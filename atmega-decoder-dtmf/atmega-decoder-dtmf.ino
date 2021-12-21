
// Код для ATmega328p/ATmega328pb
// Arduino UNO/Nano/Iskra Nano Pro
// (c) Alexander Galilov, 2021
// Скорость оцифровки (она же - частота дискретизации) сэмплов/сек (эффективная)
const float ADC_SPEED = 9615.385f;

// Продолжительность одного DTMF символа, миллисекунды
const uint16_t DTMF_MIN_DURATION_MS = 30;

// Размер буфера для данных АЦП, сэмплы
const uint16_t ADC_BUF_SIZE = (uint16_t)((uint32_t)(ADC_SPEED * DTMF_MIN_DURATION_MS / 1000));

// Количество буферов
const uint8_t ADC_N_BUFFERS = 2;

// Дисперсия сигнала для состояния "нет DTMF"
const uint16_t SILENCE_DISPERSION = 20;

// Дисперсия сигнала для состояния "есть DTMF"
const uint16_t DTMF_DISPERSION = 100;

// Уровень сигнала для "трубка поднята"
const int16_t ON_HOOK_SIGNAL_LEVEL = 100;

// Уровень сигнала для "трубка лежит"
const int16_t OFF_HOOK_SIGNAL_LEVEL = 10;

// Применяется в Q15 fixed point арифметике.
// См. https://en.wikipedia.org/wiki/Fixed-point_arithmetic
const int16_t q15_shift = 14;

const uint8_t NOT_A_BUF_NO = 100;

const int16_t MIN_POWER = 90;

// Буферы данных АЦП
uint8_t buf[ADC_N_BUFFERS][ADC_BUF_SIZE];

// Индекс используемого буфера
volatile uint8_t buf_no = 0, dont_use_buf_no = NOT_A_BUF_NO;

// Смещение внутри буфера куда будет помещен очередной байт из АЦП.
uint16_t buf_index = 0;

// Индекс буфера с готовыми данными
volatile uint8_t buf_ready_no = 0;

// Флаг готовности данных в буфере.
// Устанавливается в обработчике прерываний АЦП, сбрасывается в коде, опрашивающем этот флаг.
volatile bool is_buf_ready = false;

// Частоты DTMF.
const int16_t freqs[] = {697, 770, 852, 941, 1209, 1336, 1477, 1633};

// Массив для сохранения коэффициентов для алгоритма Герцеля - для каждой частоты отдельно.
int32_t coeffs[sizeof(freqs) / sizeof(freqs[0])];

// Вычисленные алгоритмом Герцеля мощности составляющих сигнала, соответствующих частотам freqs.
int16_t powers[sizeof(freqs) / sizeof(freqs[0])];

// Символы DTMF
const char dtmf_symbols[4][4] =
{
  {'1', '2', '3', 'A'},
  {'4', '5', '6', 'B'},
  {'7', '8', '9', 'C'},
  {'*', '0', '#', 'D'}
};

// Счетчики повторов DTMF
uint8_t dtmf_symbol_counters[4][4];


bool is_silence = true;
bool on_hook = false;

// Здесь хранится состояние регистров управления АЦП
byte saved_adcsra, saved_adcsrb, saved_admux;

// Обработчик прерываний от АЦП
ISR (ADC_vect) {
  static bool should_skip_byte = false;
  uint8_t adc_val = ADCH;
  // Защитный механизм - может использоваться в случае риска не успеть обработать
  // буфер готовых данных вовремя (до его повторного использования для новых данных с АЦП).
  if (dont_use_buf_no == buf_no) {
    return;
  }
  buf[buf_no][buf_index++] = adc_val;
  if (buf_index == ADC_BUF_SIZE) {
    buf_ready_no = buf_no;
    buf_no = (buf_no + 1) % ADC_N_BUFFERS;
    buf_index = 0;
    // Установка флага готовности данных в буфере.
    is_buf_ready = true;
  }
}

void save_adc_configuration() {
  saved_adcsra = ADCSRA;
  saved_adcsrb = ADCSRB;
  saved_admux = ADMUX;
}

void restore_adc_configuration() {
  ADCSRA = saved_adcsra;
  ADCSRB = saved_adcsrb;
  ADMUX = saved_admux;
}

void configure_adc() {
  // https://ww1.microchip.com/downloads/en/DeviceDoc/ATmega48A-PA-88A-PA-168A-PA-328-P-DS-DS40002061B.pdf, стр 257-258
  // ADLAR:1 MUX3:0 MUX2:0 MUX1:0 MUX0:0
  // ADLAR:1 включает "левое" выравнивание данных ADC. Таким образом в ADCH мы сразу
  // получает старшие 8 бит результата,
  // а 2 младших бита в ADCL не используем, т.к. они обычно самые зашумленные.
  // MUX3:0 MUX2:0 MUX1:0 MUX0:0 - Подключаем к АЦП вход ADC0. На платах Arduino
  // этот вход обычно подключен к пину Analog in A0.
  // REFS1:0 REFS1:1 - В качестве опорного источника напряжения для АЦП используем
  // напряжение питания микроконтроллера 5 вольт.
  uint8_t admux = ADMUX;
  // MUX3,MUX2,MUX1,MUX0 := 0,0,0,0,0
  bitClear(admux, MUX0);
  bitClear(admux, MUX1);
  bitClear(admux, MUX2);
  bitClear(admux, MUX3);
  // REFS0 := 1
  bitSet(admux, REFS0);
  // REFS1 := 0
  bitClear(admux, REFS1);
  // ADLAR := 1
  bitSet(admux, ADLAR);
  ADMUX = admux; // устанавливаем все биты за 1 операцию. Просто пример.

  // https://ww1.microchip.com/downloads/en/DeviceDoc/ATmega48A-PA-88A-PA-168A-PA-328-P-DS-DS40002061B.pdf, стр 258
  // Биты ADPS2:1 ADPS1:1 ADPS0:1 - установить делитель в 128 что дает
  // 16МГц / 128 = 125кГц тактовой частоты АЦП.
  // На одно преобразование Аналог -> Цифра АЦП тратит 13 своих тактов, что дает
  // в итоге скорость оцифровки 125000 / 13 = ~9615.38 выборки/сек в Free running mode.
  // ADIE:1 - Разрешаем прерывания от АЦП
  // ADATE:1 - Разрешаем автозапуск АЦП. Нужно для Free running mode.
  // ADEN:1 - Включить АЦП
  uint8_t adcsra = ADCSRA;
  bitSet(adcsra, ADIE);
  bitSet(adcsra, ADATE);
  bitSet(adcsra, ADEN);
  bitSet(adcsra, ADPS0);
  bitSet(adcsra, ADPS1);
  bitSet(adcsra, ADPS2);
  ADCSRA = adcsra;

  // https://ww1.microchip.com/downloads/en/DeviceDoc/ATmega48A-PA-88A-PA-168A-PA-328-P-DS-DS40002061B.pdf, стр 260
  // ADCSRB bits: ADTS2:0 ADTS1:0 ADTS0:0 - включить Free running mode для автоматического
  // перезапуска АЦП.
  uint8_t adcsrb = ADCSRB;
  bitClear(adcsrb, ADTS0);
  bitClear(adcsrb, ADTS1);
  bitClear(adcsrb, ADTS2);
  ADCSRB = adcsrb;

  // Запускаем цикл оцифровки сигнала на АЦП
  bitSet(ADCSRA, ADSC);
}

// https://en.wikipedia.org/wiki/Goertzel_algorithm#Applications
void goertzel(uint8_t* samples, uint8_t coeff_index)
{
  // Здесь используется сдвиг на 14 позиций при умножении для учета факта
  // масштабированности и получения произведения в формате Q15, а не Q30.
  // См. https://en.wikipedia.org/wiki/Fixed-point_arithmetic#Multiplication
  int32_t s;
  int32_t sprev = 0;
  int32_t sprev2 = 0;
  auto coeff = coeffs[coeff_index];

  for (int i = 0; i < ADC_BUF_SIZE; i++)
  {
    s = samples[i] + ((coeff * sprev) >> q15_shift) - sprev2;
    // сдвигаем элементы задержки
    sprev2 = sprev;
    sprev = s;
  }

  // Расчет мощности
  int16_t prod1 = ((sprev * sprev) >> q15_shift);
  int16_t prod2 = ((sprev2 * sprev2) >> q15_shift);
  int16_t prod3 = ((sprev * coeff) >> q15_shift);
  prod3 = ((prod3 * sprev2) >> q15_shift);
  int16_t power = prod1 + prod2 - prod3;
  if (power >= MIN_POWER) {
    powers[coeff_index] += power;
  }
}

void update_signal_state(uint8_t* samples) {
  uint32_t avg_level = 0;
  uint32_t signal_dispersion = 0;
  for (int i = 0; i < ADC_BUF_SIZE; i++) {
    avg_level += samples[i];
  }
  avg_level /= ADC_BUF_SIZE;
  for (int i = 0; i < ADC_BUF_SIZE; i++) {
    auto s = samples[i];
    auto d = (s - avg_level);
    signal_dispersion += (d * d);
  }
  signal_dispersion /= ADC_BUF_SIZE;
  if (signal_dispersion <= SILENCE_DISPERSION && !is_silence) {
    is_silence = true;
    silence_state_changed();
  } else if (signal_dispersion >= DTMF_DISPERSION && is_silence) {
    is_silence = false;
    silence_state_changed();
  }
  if (avg_level >= ON_HOOK_SIGNAL_LEVEL && !on_hook) {
    on_hook = true;
    hook_state_changed();
  } else if (avg_level <= OFF_HOOK_SIGNAL_LEVEL && on_hook) {
    on_hook = false;
    hook_state_changed();
  }
}

// Расчет коэффициентов для алгоритма Герцеля.
// См. https://en.wikipedia.org/wiki/Goertzel_algorithm#Applications
void calcCoeffs() {
  for (int8_t i = 0; i < sizeof(freqs) / sizeof(freqs[0]); i++)
  {
    // Расчитываем коэффициент и переводим его в формат с фикс. точкой Q15
    // См. https://www.allaboutcircuits.com/technical-articles/fixed-point-representation-the-q-format-and-addition-examples/
    coeffs[i] = (1L << q15_shift) * (2 * cos(2 * M_PI * (freqs[i] / ADC_SPEED)));
  }
}

void setup() {
  Serial.begin(115200);
  calcCoeffs();
  noInterrupts();
  save_adc_configuration();
  configure_adc();
  interrupts();
}

void hook_state_changed() {
  Serial.println(on_hook ? "ON-HOOK" : "OFF-HOOK");
}

void silence_state_changed() {
  if (is_silence) {
    int8_t max_power_index[2] = { -1, -1};
    for (int8_t i = 0; i < 2; i++) {
      int16_t maximum = MIN_POWER - 1;
      for (int8_t j = 0; j < sizeof(powers) / sizeof(powers[0]) / 2; j++) {
        int8_t index = i * (sizeof(powers) / sizeof(powers[0]) / 2) + j;
        if (powers[index] > maximum) {
          maximum = powers[index];
          max_power_index[i] = j;
        }
      }
    }
    for (int8_t i = 0; i < sizeof(powers) / sizeof(powers[0]); i++) {
      powers[i] = 0;
    }
    if (max_power_index[0] > -1 && max_power_index[1] > -1) {
      Serial.print(dtmf_symbols[max_power_index[0]][max_power_index[1]]);
    }
  }
}

void loop() {
  // Проверка флага готовности данных в буфере.
  if (is_buf_ready) {
    // Сброс флага готовности данных.
    is_buf_ready = false;
    // На всякий случае запрещаем менять буфер, находящийся в обработке.
    dont_use_buf_no = buf_ready_no;
    // Получаем указатель на буфер с готовыми к обработке данными.
    auto samples = buf[buf_ready_no];

    update_signal_state(samples);
    if (!is_silence) {
      // Вычисляем powers
      for (int8_t i = 0; i < sizeof(freqs) / sizeof(freqs[0]); i++) {
        goertzel(samples, i);
      }
    }
    // Разрешаем использовать буфер, бывший в обработке.
    dont_use_buf_no = NOT_A_BUF_NO;
  }
}


// Код для ATmega328p/ATmega328pb
// Arduino UNO/Nano/Iskra Nano Pro
// (c) Alexander Galilov, 2021

// Скорость оцифровки сэмплов/сек
const uint16_t ADC_SPEED = 9615;

// Продолжительность одного DTMF символа, миллисекунды
const uint16_t DTMF_MIN_LATENCY_MS = 40;

// Размер буфера для данных АЦП, сэмплы
const uint16_t ADC_BUF_SIZE = static_cast<uint16_t>(static_cast<uint32_t>(ADC_SPEED) * DTMF_MIN_LATENCY_MS / 1000U);

// Количество буферов
const uint8_t ADC_N_BUFFERS = 2;

// Буферы данных АЦП
uint8_t buf[ADC_N_BUFFERS][ADC_BUF_SIZE];

// Индекс используемого буфера
uint8_t buf_no = 0;

// Смещение внутри буфера куда будет помещен очередной байт из АЦП.
uint16_t buf_index = 0;

// Индекс буфера с готовыми данными
volatile uint8_t buf_ready_no = 0;

// Флаг готовности данных в буфере.
// Устанавливается в обработчике прерываний АЦП, сбрасывается в коде, опрашивающем этот флаг.
volatile bool is_buf_ready = false;

// Здесь храниться состояние регистров управления АЦП
byte saved_adcsra, saved_adcsrb, saved_admux;

// Обработчик прерываний от АЦП
ISR (ADC_vect) {
  uint8_t adc_val = ADCH;
  buf[buf_no][buf_index++] = adc_val;
  if (buf_index == ADC_BUF_SIZE) {
    buf_ready_no = buf_no;
    buf_no = (buf_no + 1) % ADC_N_BUFFERS;
    buf_index = 0;
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
  // в итоге скорость оцифровки 125000 / 13 = ~9615.4 выборки/сек в Free running mode.
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

void setup() {
  Serial.begin(500000);
  noInterrupts();
  save_adc_configuration();
  configure_adc();
  interrupts();
}

void loop() {
  // put your main code here, to run repeatedly:
  if (is_buf_ready) {
    is_buf_ready = false;
    for (int i = 0; i < ADC_BUF_SIZE; i++) {
      Serial.println(buf[buf_ready_no][i]);
    }
  }
}

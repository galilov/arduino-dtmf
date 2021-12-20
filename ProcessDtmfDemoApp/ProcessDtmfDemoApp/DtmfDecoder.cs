using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ProcessDtmf
{
    internal class DtmfDecoder
    {
        public delegate void SymbolReadyHandler(string c);

        // Длина буфера для сигнала одного символа, в секундах.
        private const double SymbolImageBufferLengthInSeconds = 0.03;

        // Длина буфера для сигнала паузы (тишины), в секундах.
        private const double PauseImageBufferLengthInSeconds = 0.007;

        // Минимальный порог опознавания амплитуды отдельного тона.
        private const double ToneMinAmplitudeThreshold = 3.0;

        // Максимальный порог девиации сигнала, опознаваемого как
        // пауза или тишина.
        private const double PauseMaxDeviationThreshold = 1.0;

        private const double HandsetOffHookThreshold = 180;
        private const double HandsetOnHookThreshold = 20;

        // Константы с частотами DTMF тонов, Гц.
        private const int F697 = 697;
        private const int F770 = 770;
        private const int F852 = 852;
        private const int F941 = 941;
        private const int F1209 = 1209;
        private const int F1336 = 1336;
        private const int F1477 = 1477;
        private const int F1633 = 1633;

        // Упорядоченный массив частот DTMF. Используется для удобства
        // итерации по частотам в цикле.
        private static readonly int[] _frequencies =
        {
            F697, F770, F852, F941, F1209, F1336, F1477, F1633
        };

        // Словарь соотвествия частот и символов DTMF. Частоты образуют
        // ключи словаря.
        private static readonly IDictionary<int, char> _symbols =
            new Dictionary<int, char>
            {
                {Key(F697, F1209), '1'},
                {Key(F697, F1336), '2'},
                {Key(F697, F1477), '3'},
                {Key(F697, F1633), 'A'},
                {Key(F770, F1209), '4'},
                {Key(F770, F1336), '5'},
                {Key(F770, F1477), '6'},
                {Key(F770, F1633), 'B'},
                {Key(F852, F1209), '7'},
                {Key(F852, F1336), '8'},
                {Key(F852, F1477), '9'},
                {Key(F852, F1633), 'C'},
                {Key(F941, F1209), '*'},
                {Key(F941, F1336), '0'},
                {Key(F941, F1477), '#'},
                {Key(F941, F1633), 'D'},
            };

        // Ссылка на входную очередь с сэмплами сигнала.
        private readonly BlockingCollection<byte> _inputSamplesQueue;

        // Выходная очередь с декодированными символами DTMF.
        private readonly BlockingCollection<string> _outputDetectedCharactersQueue =
            new BlockingCollection<string>();

        // Ссылка на обработчик, которому передаются декодированные символы.
        private readonly SymbolReadyHandler _handler;

        // Нить, в которой выполняется декодирование входного потока данных.
        private Thread _thrInputDataProcessing;

        // Нить, в которой происходит вызов обработчика и передача ему
        // результирующих символов DTMF.
        private Thread _thrOutputDataProcessing;

        // Размер сигнального образа символа DTMF в сэмплах.
        private readonly int _symbolImageSizeInSamples;

        // Размер сигнального образа паузы (тишины) между символами в
        // сэмплах.
        private readonly int _pauseImageSizeInSamples;

        // Списки-буферы для хранения фрагментов сигнала и (предполагаемой)
        // паузы в процессе обработки. Возможно, здесь лучше использовать
        // кольцевой буфер, но его в готовом виде нет, а делать не хочу.
        private readonly LinkedList<byte>
            _symbolImage = new LinkedList<byte>(),
            _pauseImage = new LinkedList<byte>();

        // Счетчики статистики распознавания. В случае, если продолжительность
        // непрерывного символьного сигнала превышает длину сигнального буфера,
        // что бывает почти всегда, алгоритм последовательно детектирует
        // несколько символов. В нормальном случае они будут одинаковыми, но
        // иногда, например из-за помех на линии или переходных процессов в
        // начале и в конце генерации, детектор распознает отличающиеся символы.
        // Чтобы повысить точность детектирования, ведется подсчет количества
        // значений обнаруженных символов. В итоге побеждает тот, который
        // детектировался чаще других.
        private readonly Dictionary<char, int> _foundSymbols = new Dictionary<char, int>();

        // Источник токенов отмены используется для прекращения работы
        // нитей. При необходимости, обеспечивает разблокировку
        // очередей, выбрасывая исключение OperationCanceledException
        // из заблокированных методов вроде Take().
        private CancellationTokenSource _cancellationTokenSource;

        // Здесь появляются амплитуды гармоник, соответствующих частотам
        // из _frequencies. Для заполения массива вызывается
        // DoFourierTransformForDtmf()
        private readonly double[] _amplitudes = new double[_frequencies.Length];

        private volatile bool _handsetIsOnHook = true; // тел. трубка положена

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="inputSamplesQueue">Очередь с сэмплами от SerialPortReader</param>
        /// <param name="frameRate">Частота сэмплирования, выборок/секунду.</param>
        /// <param name="handler">Обработчик, которому передаются обнаруженные символы.</param>
        public DtmfDecoder(BlockingCollection<byte> inputSamplesQueue, float frameRate,
            SymbolReadyHandler handler)
        {
            _inputSamplesQueue = inputSamplesQueue;
            _handler = handler;
            // Рассчитываем размер буферов (необходимое количество
            // сэмплов) на основе частоты дискретизации и
            // продолжительности фрагментов, необходимых для работы
            // детектора (дискретного преобразования Фурье).
            _symbolImageSizeInSamples = (int)Math.Round(frameRate * SymbolImageBufferLengthInSeconds);
            _pauseImageSizeInSamples = (int)Math.Round(frameRate * PauseImageBufferLengthInSeconds);
        }

        /// <summary>
        /// Запуск обработки.
        /// </summary>
        public void Start()
        {
            // Если не запущено...
            if (_thrInputDataProcessing == null)
            {
                // Создаем источник токенов отмены.
                _cancellationTokenSource = new CancellationTokenSource();
                // и пару нитей: для обработки входных данных
                // и для передачи детектированных символов клиентскому
                // обработчику.
                _thrInputDataProcessing = new Thread(InputDataProcessing);
                _thrOutputDataProcessing = new Thread(OutputDataProcessing);
                // Запускаем нити.
                _thrInputDataProcessing.Start();
                _thrOutputDataProcessing.Start();
            }
        }

        /// <summary>
        /// Остановка обработки.
        /// </summary>
        public void Stop()
        {
            // Если нить активна...
            if (_thrInputDataProcessing != null && _thrInputDataProcessing.IsAlive)
            {
                // Сигнал через источник токенов отмены на отмену работы
                // нитей и разблокировку очередей.
                _cancellationTokenSource.Cancel();
                // Ожидаем завершения нитей.
                _thrInputDataProcessing.Join();
                _thrOutputDataProcessing.Join();
                // Освобождаем Disposable ресурсы источника токенов отмены.
                _cancellationTokenSource.Dispose();
                // Обнуляем ссылку на нить чтобы избежать повторных
                // попыток остановки.
                _thrInputDataProcessing = null;
            }
        }

        /// <summary>
        /// Метод нити обработки входного потока сэмплов.
        /// </summary>
        private void InputDataProcessing()
        {
            var token = _cancellationTokenSource.Token;
            try
            {
                while (!token.IsCancellationRequested) // Можем работать?
                {
                    // Если НЕдостаточно сэмплов для обнаружения паузы... 
                    if (_pauseImage.Count < _pauseImageSizeInSamples)
                    {
                        // ...перекладываем сэмплы из входной очереди в буфер
                        // обнаружения тишины.
                        //
                        // Take(token) выбрасывает исключение OperationCanceledException
                        // в случае отмены через _cancellationTokenSource.Cancel()
                        var sample = _inputSamplesQueue.Take(token);
                        _pauseImage.AddLast(sample);
                    }
                    else if (IsItPause()) // Пауза?
                    {
                        // Да, пауза!
                        // Удаляем все запасенные сэмплы, т.к.
                        // Паузу мы нашли...
                        _pauseImage.Clear();
                        // ...а предыдущий блок сэмплов для детектирования
                        // символа уже был обработан либо не актуален.
                        _symbolImage.Clear();
                        // Выдаем обнаруженный к этому моменту символ, т.к.
                        // пауза означает, что за ней следует новый символ.
                        ProduceSymbol(token);
                    }
                    else // нет, не пауза
                    {
                        // "сдвигаем" до 1/4 накопленных ранее для обнаружения
                        // паузы сэмплов в буфер ДПФ и этим освобождаем место
                        // в буфере обнаружения паузы.
                        while (_pauseImage.Count > _pauseImageSizeInSamples * 3 / 4
                               && _symbolImage.Count < _symbolImageSizeInSamples)
                        {
                            var firstElement = _pauseImage.First;
                            _symbolImage.AddLast(firstElement.Value);
                            _pauseImage.Remove(firstElement);
                        }

                        // Если буфер ДПФ достаточно заполнен...
                        if (_symbolImage.Count >= _symbolImageSizeInSamples)
                        {
                            // ...то выполоняем его обработку.
                            ProcessSymbolBuffer();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ничего тут не делаем - просто уходим
            }
        }

        /// <summary>
        /// Обрабатываем образ (буфер) сигнала с, предположительно, сэмплами
        /// DTMF.
        /// </summary>
        private void ProcessSymbolBuffer()
        {
            // Вычисляем амплитуды гармоник, соответствующих
            // частотам DTMF.
            DoFourierTransformForDtmf();
            // Ищем символ.
            FindMatchingSymbol();
            // Выбрасываем 1/4 самых "старых" сэмплов с головы
            // чтобы было место для загрузки новых с хвоста.
            while (_symbolImage.Count > _symbolImageSizeInSamples * 3 / 4)
            {
                _symbolImage.RemoveFirst();
            }
        }

        /// <summary>
        /// Применяем ДПФ для вычисления амплитуд гармоник, соответствующих
        /// частотам DTMF.
        /// </summary>
        private void DoFourierTransformForDtmf()
        {
            // Цикл по массиву частот
            for (var freqIndex = 0; freqIndex < _frequencies.Length; freqIndex++)
            {
                // Получаем частоту из массива
                var f = _frequencies[freqIndex];
                // Вычислям порядковый номер ближайшей гармоники на участке образа
                // сигнала продолжительностью SymbolImageBufferLengthInSeconds
                // секунд.
                var k = Math.Round(f * SymbolImageBufferLengthInSeconds);
                // Вычисляем соответствующую этой гармонике циклическую частоту.
                // Другими словами, делаем k оборотов на за
                // SymbolImageBufferLengthInSeconds секунд.
                var w = 2 * Math.PI * k;
                // Накапливаем суммы по косинусам и синусам.
                // Никаких комплексных величин в явном виде!
                double sumCosWt = 0.0, sumSinWt = 0.0;
                // Номер семпла, нужен для вычисления поворота на комплексной
                // плоскости.
                double i = 0;
                // Итерация по сэмплам
                foreach (var sample in _symbolImage)
                {
                    // Вычислям относительное время в интервале от [0..1), где
                    // 1 соответствует концу образа (буфера) сигнала.
                    var t = i / _symbolImageSizeInSamples;
                    // Вычисляем поворот, соответствующий относительному времени t.
                    var wt = w * t;
                    // "горизонтальная" составляющая единичного вектора, повернутого на wt.
                    var cosWt = Math.Cos(wt);
                    // "вертикальная" составляющая единичного вектора, повернутого на wt.
                    var sinWt = Math.Sin(wt);
                    // Умножаем вектор на величину сигнала (сэмпл) и суммируем результаты
                    // отдельно по составляющим.
                    sumCosWt += cosWt * sample;
                    sumSinWt += sinWt * sample;
                    // Следующий шаг поворота.
                    i++;
                }

                // Вычисляем координаты "центра масс" фигуры вращения
                var avgCosWt = sumCosWt / _symbolImageSizeInSamples;
                var avgSinWt = sumSinWt / _symbolImageSizeInSamples;
                // Сохраняем амплитуду гармоники k в массиве амплитуд.
                // Вычисление по теореме Пифагора. Катеты - проекции масштабированного
                // в sample раз повернутого единичного вектора на реальную (X) и мнимую
                // (Y) оси.
                _amplitudes[freqIndex] = Math.Sqrt(avgCosWt * avgCosWt + avgSinWt * avgSinWt);
            }
        }

        /// <summary>
        /// Выбирает среди найденных символов-кандидатов наиболее вероятный
        /// и помещает его в выходную очередь.
        /// Очищает накопленные счетчики.
        /// </summary>
        /// <param name="token">Токе отмены, разблокирующий очередь в случае
        /// требования остановки нити.</param>
        private void ProduceSymbol(CancellationToken token)
        {
            if (_foundSymbols.Count > 0)
            {
                var max = _foundSymbols.Max(kvp => kvp.Value);
                var symbol = _foundSymbols.First(kvp => kvp.Value == max).Key;
                _foundSymbols.Clear();
                _outputDetectedCharactersQueue.Add(symbol.ToString(), token);
            }
        }

        /// <summary>
        /// Проверяем, есть ли в образе сигнала паузы ожидаемая пауза
        /// (тишина)
        /// </summary>
        /// <returns>true - пауза обнаружена</returns>
        private bool IsItPause()
        {
            // Вычисляем среднее значение сигнала
            var avgLevel = _pauseImage.Select(x => (int)x).Average();
            if (avgLevel > HandsetOffHookThreshold && _handsetIsOnHook)
            {
                _handsetIsOnHook = false;
                _outputDetectedCharactersQueue.Add("\nOFF-HOOK\n");
            }
            else if (avgLevel < HandsetOnHookThreshold && !_handsetIsOnHook)
            {
                _handsetIsOnHook = true;
                _outputDetectedCharactersQueue.Add("\nON-HOOK\n");
            }
            // Вычисляем среднеквадратическое отклонение как квадратный
            // корень из дисперсии.
            // См. https://ru.wikipedia.org/wiki/%D0%94%D0%B8%D1%81%D0%BF%D0%B5%D1%80%D1%81%D0%B8%D1%8F_%D1%81%D0%BB%D1%83%D1%87%D0%B0%D0%B9%D0%BD%D0%BE%D0%B9_%D0%B2%D0%B5%D0%BB%D0%B8%D1%87%D0%B8%D0%BD%D1%8B
            var deviation = Math.Sqrt(
                _pauseImage
                    .Select(x => (x - avgLevel) * (x - avgLevel))
                    .Sum() / _pauseImage.Count);
            return deviation <= PauseMaxDeviationThreshold;
        }

        /// <summary>
        /// Метод нити, вызывающей обработчик найденных символов.
        /// </summary>
        private void OutputDataProcessing()
        {
            var token = _cancellationTokenSource.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Take(token) выбрасывает исключение OperationCanceledException
                    // в случае отмены через _cancellationTokenSource.Cancel()
                    _handler?.Invoke(_outputDetectedCharactersQueue.Take(token));
                }
            }
            catch (OperationCanceledException)
            {
                // Ничего тут не делаем - просто уходим
            }
        }

        /// <summary>
        /// Находим символ, соответствующий вычисленным ранее амплитудам
        /// составляющих сигнал гармоник.
        /// </summary>
        private void FindMatchingSymbol()
        {
            // Ищем индекс элемента, соответствующий самой большой амплитуде.
            var maxFreq1Index = FindMaxElementIndex(_amplitudes);
            // Сохраняем значение макс. амплитуды.
            var max1FreqAmplitude = _amplitudes[maxFreq1Index];
            // Заменяем элемент на заведомо невозможное для амплитуды
            // малое значение.
            _amplitudes[maxFreq1Index] = -1;

            // Ищем следующую по величине амплитуду
            var maxFreq2Index = FindMaxElementIndex(_amplitudes);
            // Сохраняем значение второй амплитуды
            var max2FreqAmplitude = _amplitudes[maxFreq2Index];
            // Заменяем элемент на заведомо невозможное для амплитуды
            // малое значение.
            _amplitudes[maxFreq2Index] = -1;

            // Сравниваем максимальную из оставшихся амплитуд с половиной порога. 
            if (_amplitudes.Max() > ToneMinAmplitudeThreshold / 2)
            {
                // Слишком сильные "нецелевые" гармоники.
                // Похоже, что это - шум. Прекращаем обработку.
                // PS А еще это может быть следствием "растекания" сигнала.
                return;
            }

            // Если найденные первый и второй максимумы больше или равны порогу...
            if (max1FreqAmplitude >= ToneMinAmplitudeThreshold
                && max2FreqAmplitude >= ToneMinAmplitudeThreshold)
            {
                // ...вероятно, найдены частоты DTMF символа, ищем соответствие
                // в словаре символов.
                var loFreq = Math.Min(_frequencies[maxFreq1Index], _frequencies[maxFreq2Index]);
                var hiFreq = Math.Max(_frequencies[maxFreq1Index], _frequencies[maxFreq2Index]);
                if (_symbols.TryGetValue(Key(loFreq, hiFreq), out var symbol))
                {
                    // Соответствие найдено, увеличиваем счетчик для найденного
                    // символа чтобы повысить точность детектирования.
                    if (!_foundSymbols.TryGetValue(symbol, out var n))
                    {
                        // Нет соответствующего счетчика - инициализируем новый.
                        n = 0;
                    }

                    // Увеличиваем счетчик.
                    _foundSymbols[symbol] = n + 1;
                }
            }
        }

        /// <summary>
        /// Служебный метод поиска ИНДЕКСА максимального элемента
        /// последовательности sequence.
        /// </summary>
        /// <param name="sequence">Последоватеьность, в которой нужно
        /// найти индекс максимального элемента.</param>
        /// <returns>Найденный индекс или -1, если последовательность
        /// пустая или все её элементы равны double.MinValue</returns>
        private static int FindMaxElementIndex(IEnumerable<double> sequence)
        {
            var maxIndex = -1;
            var maxValue = double.MinValue;

            var index = 0;
            foreach (var value in sequence)
            {
                if (value > maxValue)
                {
                    maxIndex = index;
                    maxValue = value;
                }

                index++;
            }

            return maxIndex;
        }

        private static int Key(int low, int high)
        {
            return low + 1000 * high;
        }
    }
}
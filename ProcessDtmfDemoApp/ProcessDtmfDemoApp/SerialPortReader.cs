using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;

namespace ProcessDtmf
{
    internal class SerialPortReader
    {
        // Объект, предоставляющий коммуникацию с последовательным
        // портом.
        private readonly SerialPort _serialPort;

        // Нить, в которой читаются данные из последовательного порта
        // и отправляются в очередь на обработку.
        private Thread _thread;

        // Источник токенов отмены используется для прекращения работы
        // нитей. При необходимости, обеспечивает разблокировку
        // очередей, выбрасывая исключение OperationCanceledException
        // из заблокированных методов вроде Take() или Add().
        private CancellationTokenSource _cancellationTokenSource;

        // Очередь, в которую добавляются сэмплы из порта.
        private readonly BlockingCollection<byte> _queue;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="serialPort">Объект, связанный с последовательным порторм.</param>
        /// <param name="queue">Очередь, куда будут добавлсяться полученные сэмплы.</param>
        public SerialPortReader(SerialPort serialPort, BlockingCollection<byte> queue)
        {
            _serialPort = serialPort;
            _queue = queue;
        }

        /// <summary>
        /// Запуск обработки.
        /// </summary>
        public void Start()
        {
            // Если не запущено...
            if (_thread == null)
            {
                // Если порт НЕ отрыт...
                if (!_serialPort.IsOpen)
                {
                    // ...открываем его.
                    _serialPort.Open();
                }

                // Создаем источник токенов отмены.
                _cancellationTokenSource = new CancellationTokenSource();
                // Создаем нить читателя.
                _thread = new Thread(Reader);
                // Запускаем нить.
                _thread.Start();
            }
        }

        public void Stop()
        {
            // Если нить активна...
            if (_thread != null && _thread.IsAlive)
            {
                // Сигнал через источник токенов отмены на отмену работы
                // нитей и разблокировку очереди.
                _cancellationTokenSource.Cancel();
                // Ожидаем завершения нити.
                _thread.Join();
                // Освобождаем Disposable ресурсы источника токенов отмены.
                _cancellationTokenSource.Dispose();
                // Если порт открыт...
                if (_serialPort.IsOpen)
                {
                    // ...закрываем его.
                    _serialPort.Close();
                }

                // Обнуляем ссылку на нить чтобы избежать повторных
                // попыток остановки.
                _thread = null;
            }
        }

        private void Reader()
        {
            var token = _cancellationTokenSource.Token;
            try
            {
                while (!token.IsCancellationRequested) // Можем работать?
                {
                    var b = _serialPort.ReadByte();
                    // Если есть новый сэмпл, то отправляем его в очередь.
                    if (b != -1)
                    {
                        // Add выбрасывает исключение OperationCanceledException
                        // в случае отмены через token
                        _queue.Add((byte) b, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ничего тут не делаем - просто уходим
            }
        }
    }
}
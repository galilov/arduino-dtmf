using System;
using System.Collections.Concurrent;
using System.IO.Ports;

namespace ProcessDtmf
{
    public static class Program
    {
        // Последовательный порт, связанный с контроллером Arduino.
        private const string PortName = "COM5";

        // Скорость порта, бод.
        private const int PortBaudRate = 115200;

        // Частота дискретизации (сэмплирования), выборок в секунду.
        private const float FrameRate = 9615.38f;

        // Объект, предоставляющий коммуникацию с последовательным портом.
        private static readonly SerialPort _serialPort = new SerialPort();

        // Через эту очередь читатель данных из порта передает сэмплы в декодер.
        private static readonly BlockingCollection<byte> _queue = new BlockingCollection<byte>();

        // Читатель данных из порта.
        private static readonly SerialPortReader _serialPortReader = new SerialPortReader(_serialPort, _queue);

        // Декодер сигналов DTMF.
        private static readonly DtmfDecoder _dtmfDecoder = new DtmfDecoder(_queue, FrameRate, Handler);

        public static void Main(string[] args)
        {
            ConfigureSerialPort();
            _serialPortReader.Start();
            _dtmfDecoder.Start();
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
            _dtmfDecoder.Stop();
            _serialPortReader.Stop();
        }

        // Настраиваем последовательный порт в режим, соответствующий
        // настройкам микроконтроллера.
        private static void ConfigureSerialPort()
        {
            _serialPort.PortName = PortName;
            _serialPort.BaudRate = PortBaudRate;
            // 8N1 по-умолчанию для Ардуино
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            // На всякий случай, чтобы не зависать на чтении данных,
            // когда их почему-то нет.
            _serialPort.ReadTimeout = 100;
        }

        /// <summary>
        /// Клиентский обработчик, получающий символы DTMF.
        /// </summary>
        /// <param name="c">Символ DTMF.</param>
        private static void Handler(char c)
        {
            Console.Write(c);
        }
    }
}
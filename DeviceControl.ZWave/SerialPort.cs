using RJCP.IO.Ports;
using System;
using System.IO;
using ZWaveLibrary = ZWave;

namespace DeviceControl.ZWave
{
    /// <summary>
    /// Wrapper to use <see cref="SerialPortStream"/> for the serial port due to lack of mac/linux support
    /// in .net core for <see cref="System.IO.Ports"/>.
    /// </summary>
    internal class SerialPort : ZWaveLibrary.Channel.ISerialPort, IDisposable
    {
        private readonly SerialPortStream _serialPortStream;

        public SerialPort(string port)
        {
            _serialPortStream = new SerialPortStream(port, 115200, 8, Parity.None, StopBits.One);
        }

        public void Dispose()
        {
            _serialPortStream.Dispose();
        }

        public Stream InputStream => _serialPortStream;

        public Stream OutputStream => _serialPortStream;

        public void Open()
        {
            _serialPortStream.Open();
            _serialPortStream.DiscardInBuffer();
            _serialPortStream.DiscardOutBuffer();
        }

        public void Close()
        {
            _serialPortStream.Close();
        }
    }
}

using RJCP.IO.Ports;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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


        // having bugs with asynchronousness downstream (timeout exceptions and not being able to receive events)
        // so I made these synchronous as a temporary workaround
        public Stream InputStream => new SynchronousStream(_serialPortStream);
        public Stream OutputStream => new SynchronousStream(_serialPortStream);

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

        private class SynchronousStream : Stream
        {
            private readonly Stream _stream;

            public SynchronousStream(Stream input)
            {
                _stream = input;
            }

            public override bool CanRead => _stream.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => _stream.CanRead;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return Task.FromResult(Read(buffer, offset, count));
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                try
                {
                    return _stream.Read(buffer, offset, count);
                }
                catch (Exception ex)
                {
                    throw new IOException("Serial port read failed.", ex);
                }
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                Write(buffer, offset, count);
                return Task.CompletedTask;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                try
                {
                    _stream.Write(buffer, offset, count);
                }
                catch (Exception ex)
                {
                    throw new IOException("Serial port write failed.", ex);
                }
            }

            public override void Flush() => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotImplementedException();
            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => throw new NotSupportedException();
            public override void EndWrite(IAsyncResult asyncResult) => throw new NotSupportedException();
            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => throw new NotSupportedException();
            public override int EndRead(IAsyncResult asyncResult) => throw new NotSupportedException();
            public override void WriteByte(byte value) => throw new NotSupportedException();
            public override int ReadByte() => throw new NotSupportedException();
            public override Task FlushAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
            public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => throw new NotSupportedException();
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LightControl.Core
{
    /// <summary>
    /// Listens to device messages via UDP at a specified port.
    /// </summary>
    public sealed class UdpMessageListener : IDisposable
    {
        public class MessageReceivedArgs : EventArgs
        {
            public MessageReceivedArgs(IPAddress address, string[] data)
            {
                Address = address;
                Data = data;
            }

            /// <summary>
            /// IP address of the sender.
            /// </summary>
            public IPAddress Address { get; }

            /// <summary>
            /// UDP message data split by newline.
            /// </summary>
            public string[] Data { get; }
        }

        private readonly UdpClient _udpClient;
        private static readonly Regex _splitRegex = new Regex("\r?\n");

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port">UDP port to listen on.</param>
        public UdpMessageListener(int port)
        {
            _udpClient = new UdpClient();
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            _udpClient.JoinMulticastGroup(IPAddress.Parse("239.255.255.250"));
            Task.Run(() => StartListening());
        }

        /// <summary>
        /// Occurs when a message was received.
        /// </summary>
        public event EventHandler<MessageReceivedArgs> MessageReceived;

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            _udpClient.Dispose();
        }

        private async Task StartListening()
        {
            while(true)
            {
                try
                {
                    while (true)
                    {
                        var result = await _udpClient.ReceiveAsync();
                        var data = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length);
                        MessageReceived?.Invoke(this, new MessageReceivedArgs(result.RemoteEndPoint.Address, _splitRegex.Split(data)));
                    }
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error listening for UDP message: " + ex.Message);

                    // wait and try again
                    await Task.Delay(10_000);
                }
            }
        }
    }
}

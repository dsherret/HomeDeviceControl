using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LightControl.Communication.Common
{
    /// <summary>
    /// Listens to device advertisements via UDP at a specified port.
    /// </summary>
    public sealed class UdpAdvertisementListener : IDisposable
    {
        public class AdvertisementReceivedArgs : EventArgs
        {
            public AdvertisementReceivedArgs(IPAddress address, byte[] data)
            {
                Address = address;
                Data = data;
            }

            public IPAddress Address { get; }
            public byte[] Data { get; }
        }

        private readonly UdpClient _udpClient;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port">UDP port to listen on.</param>
        public UdpAdvertisementListener(int port)
        {
            _udpClient = new UdpClient();
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            _udpClient.JoinMulticastGroup(IPAddress.Parse("239.255.255.250"));
            Task.Run(() => StartListening());
        }

        /// <summary>
        /// Occurs when an advertisement was received.
        /// </summary>
        public event EventHandler<AdvertisementReceivedArgs> AdvertisementReceived;

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            _udpClient.Dispose();
        }

        private async Task StartListening()
        {
            try
            {
                while (true)
                {
                    var result = await _udpClient.ReceiveAsync();
                    AdvertisementReceived?.Invoke(this, new AdvertisementReceivedArgs(result.RemoteEndPoint.Address, result.Buffer.ToArray()));
                }
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error listening for UDP advertisement: " + ex.Message);

                // wait and try again
                await Task.Delay(30_000);
                await StartListening();
            }
        }
    }
}

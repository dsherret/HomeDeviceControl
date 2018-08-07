using DeviceControl.Core;
using DeviceControl.Core.LightBulbs;
using DeviceControl.Core.Utils;
using System;
using System.Linq;
using YeelightAPI;

namespace DeviceControl.Plugin.Yeelight
{
    public sealed class YeelightBulbDiscoverer : ILightBulbDiscoverer, IDisposable
    {
        private readonly UdpMessageListener _udpMessageListener = new UdpMessageListener(1982);

        public YeelightBulbDiscoverer()
        {
            DiscoverLights();
            _udpMessageListener.MessageReceived += UdpMessageListener_MessageReceived;
        }

        public void Dispose()
        {
            _udpMessageListener.MessageReceived -= UdpMessageListener_MessageReceived;
            _udpMessageListener.Dispose();
        }

        public event EventHandler<LightBulbEventArgs> Discovered;

        private async void DiscoverLights()
        {
            var devices = await DeviceLocator.Discover();

            foreach (var device in devices.ToArray()) // collection could be modified by library so cache its current state while iterating
                FireDiscovered(device.Id, device);
        }

        private void UdpMessageListener_MessageReceived(object sender, UdpMessageListener.MessageReceivedArgs args)
        {
            var isAdvertisement = args.Data.Any(line => line == "NTS: ssdp:alive");
            if (!isAdvertisement)
                return;

            const string idPrefix = "id: ";
            var id = args.Data.First(line => line.StartsWith(idPrefix)).Substring(idPrefix.Length);

            FireDiscovered(id, new Device(args.Address.ToString()));
        }

        private async void FireDiscovered(string id, Device device)
        {
            await device.Connect();
            Discovered?.Invoke(this, new LightBulbEventArgs(new YeelightBulb(GuidUtils.StringToGuid(id), device)));
        }
    }
}

using LightControl.Core.LightBulbs;
using LightControl.Core.Utils;
using System;
using System.Drawing;
using System.Threading.Tasks;
using YeelightAPI;

namespace LightControl.Plugin.Yeelight
{
    public sealed class YeelightBulb : ILightBulb
    {
        private readonly Device _device;

        internal YeelightBulb(Guid id, Device device)
        {
            Id = id;
            _device = device;
        }

        public void Dispose()
        {
            _device.Dispose();
        }

        public Guid Id { get; }
        public bool IsConnected => _device.IsConnected;

        public Task ConnectAsync()
        {
            // the API will disconnect then reconnect so avoid this behaviour
            if (IsConnected)
                return Task.CompletedTask;

            return _device.Connect();
        }

        public Task TurnOnAsync()
        {
            return _device.TurnOn(1000);
        }

        public Task TurnOffAsync()
        {
            return _device.TurnOff(1000);
        }

        public Task SetRGBColorAsync(Color color)
        {
            return _device.SetRGBColor(color.R, color.G, color.B, 1000);
        }
    }
}

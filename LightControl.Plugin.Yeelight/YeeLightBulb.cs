using LightControl.Core.LightBulbs;
using System.Threading.Tasks;
using YeelightAPI;

namespace LightControl.Plugin.Yeelight
{
    public sealed class YeelightBulb : ILightBulb
    {
        private readonly Device _device;

        internal YeelightBulb(Device device)
        {
            _device = device;
        }

        public void Dispose()
        {
            _device.Dispose();
        }

        public string Id => _device.Id;

        public Task ConnectAsync()
        {
            return _device.Connect();
        }

        public Task TurnOnAsync()
        {
            return _device.TurnOn();
        }

        public Task TurnOffAsync()
        {
            return _device.TurnOff();
        }

        public Task SetRGBColorAsync(int r, int g, int b)
        {
            return _device.SetRGBColor(r, g, b);
        }
    }
}

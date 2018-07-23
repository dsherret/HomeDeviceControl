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

        public Task Connect()
        {
            return _device.Connect();
        }

        public Task TurnOn()
        {
            return _device.TurnOn();
        }

        public Task TurnOff()
        {
            return _device.TurnOff();
        }

        public Task SetRGBColor(int r, int g, int b)
        {
            return _device.SetRGBColor(r, g, b);
        }
    }
}

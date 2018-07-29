using LightControl.Core.LightBulbs;
using System.Drawing;
using System.Threading.Tasks;

namespace LightControl.LightBulbs
{
    public class LightBulbWrapper
    {
        private readonly ILightBulb _lightBulb;
        private bool _isColorDirty;
        private bool _isBrightnessDirty;
        private bool _isTemperatureDirty;

        public LightBulbWrapper(ILightBulb lightBulb)
        {
            _lightBulb = lightBulb;
            InitializeDirtyListeners();
        }

        public Task<bool> GetPowerAsync()
        {
            return _lightBulb.GetPowerAsync();
        }

        public Task<int> GetBrightnessAsync()
        {
            return _lightBulb.GetBrightnessAsync();
        }

        public Task<Color> GetColorAsync()
        {
            return _lightBulb.GetColorAsync();
        }

        public Task<int> GetTemperatureAsync()
        {
            return _lightBulb.GetTemperatureAsync();
        }

        public Task SetPowerAsync(bool power)
        {
            return _lightBulb.SetPowerAsync(power);
        }

        public Task SetColorAsync(Color color)
        {
            if (_isColorDirty)
                return Task.CompletedTask;
            return _lightBulb.SetColorAsync(color);
        }

        public Task SetBrightnessAsync(int brightness)
        {
            if (_isBrightnessDirty)
                return Task.CompletedTask;
            return _lightBulb.SetBrightnessAsync(brightness);
        }

        public Task SetTemperatureAsync(int temperature)
        {
            if (_isTemperatureDirty)
                return Task.CompletedTask;
            return _lightBulb.SetTemperatureAsync(temperature);
        }

        private void InitializeDirtyListeners()
        {
            _lightBulb.PowerStatusChanged += (sender, e) =>
            {
                _isColorDirty = false;
                _isBrightnessDirty = false;
                _isTemperatureDirty = false;
            };

            _lightBulb.ColorChanged += (sender, e) => _isColorDirty = true;
            _lightBulb.TemperatureChanged += (sender, e) => _isTemperatureDirty = true;
            _lightBulb.BrightnessChanged += (sender, e) => _isBrightnessDirty = true;
        }
    }
}

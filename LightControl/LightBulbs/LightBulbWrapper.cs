using LightControl.Core.LightBulbs;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace LightControl.LightBulbs
{
    public class LightBulbWrapper
    {
        private readonly ILightBulb _lightBulb;
        private bool _isDirty;

        public LightBulbWrapper(ILightBulb lightBulb)
        {
            _lightBulb = lightBulb;
            InitializeDirtyListeners();
        }

        public Guid Id => _lightBulb.Id;

        public event EventHandler<LightBulbWrapperEventArgs> Connected;
        public event EventHandler<LightBulbWrapperEventArgs> PowerChanged;

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
            if (_isDirty)
                return Task.CompletedTask;
            return _lightBulb.SetColorAsync(color);
        }

        public Task SetBrightnessAsync(int brightness)
        {
            if (_isDirty)
                return Task.CompletedTask;
            return _lightBulb.SetBrightnessAsync(brightness);
        }

        public Task SetTemperatureAsync(int temperature)
        {
            if (_isDirty)
                return Task.CompletedTask;
            return _lightBulb.SetTemperatureAsync(temperature);
        }

        private void InitializeDirtyListeners()
        {
            _lightBulb.PowerStatusChanged += (sender, e) =>
            {
                _isDirty = false;
                PowerChanged?.Invoke(this, new LightBulbWrapperEventArgs(this));
            };
            _lightBulb.Connected += (sender, e) =>
            {
                _isDirty = false;
                Connected?.Invoke(this, new LightBulbWrapperEventArgs(this));
            };

            _lightBulb.ColorChanged += (sender, e) => _isDirty = true;
            _lightBulb.TemperatureChanged += (sender, e) => _isDirty = true;
            _lightBulb.BrightnessChanged += (sender, e) => _isDirty = true;
        }
    }
}

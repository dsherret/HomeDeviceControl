using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace LightControl.Core.LightBulbs
{
    internal class LightBulbContainer : ILightBulb
    {
        private class State
        {
            public bool IsPoweredOn { get; set; }
            public Color Color { get; set; }
            public int Temperature { get; set; }
            public int Brightness { get; set; }
        }

        private readonly State _state = new State();
        private readonly HashSet<string> _pendingState = new HashSet<string>();
        private readonly object _lock = new object();
        private ILightBulb _lightBulb;

        public LightBulbContainer(Guid id)
        {
            Id = id;
        }

        internal void SetLightBulb(ILightBulb lightBulb)
        {
            lock (_lock)
            {
                if (_lightBulb != null)
                {
                    _lightBulb.Connected -= Connected;
                    _lightBulb.PowerStatusChanged -= LightBulb_PowerStatusChanged;
                    _lightBulb.BrightnessChanged -= LightBulb_BrightnessChanged;
                    _lightBulb.TemperatureChanged -= LightBulb_TemperatureChanged;
                    _lightBulb.ColorChanged -= LightBulb_ColorChanged;
                }

                _lightBulb?.Dispose();
                _lightBulb = lightBulb;

                if (_lightBulb != null)
                {
                    _lightBulb.Connected += Connected;
                    _lightBulb.PowerStatusChanged += LightBulb_PowerStatusChanged;
                    _lightBulb.BrightnessChanged += LightBulb_BrightnessChanged;
                    _lightBulb.TemperatureChanged += LightBulb_TemperatureChanged;
                    _lightBulb.ColorChanged += LightBulb_ColorChanged;
                }
            }
            var unusedTask = ConnectAsync();
        }

        public event EventHandler Connected;
        public event EventHandler<LightBulbPropertyChangedEventArgs<bool>> PowerStatusChanged;
        public event EventHandler<LightBulbPropertyChangedEventArgs<int>> BrightnessChanged;
        public event EventHandler<LightBulbPropertyChangedEventArgs<int>> TemperatureChanged;
        public event EventHandler<LightBulbPropertyChangedEventArgs<Color>> ColorChanged;

        public Guid Id { get; }
        public bool IsConnected => _lightBulb?.IsConnected ?? false;

        public async Task ConnectAsync()
        {
            await (_lightBulb?.ConnectAsync() ?? Task.CompletedTask);
            await SyncState();
            Connected?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _lightBulb?.Dispose();
        }

        public Task SetPowerAsync(bool power)
        {
            _state.IsPoweredOn = power;
            _pendingState.Add(nameof(_state.IsPoweredOn));
            return SyncState();
        }

        public Task SetBrightnessAsync(int brightness)
        {
            _state.Brightness = brightness;
            _pendingState.Add(nameof(_state.Brightness));
            return SyncState();
        }

        public Task SetColorAsync(Color color)
        {
            _state.Color = color;
            _pendingState.Add(nameof(_state.Color));
            return SyncState();
        }

        public Task SetTemperatureAsync(int temperature)
        {
            _state.Temperature = temperature;
            _pendingState.Add(nameof(_state.Temperature));
            return SyncState();
        }

        public Task<bool> GetPowerAsync()
        {
            return GetResultOrDefault(_lightBulb?.GetPowerAsync(), false);
        }

        public Task<int> GetBrightnessAsync()
        {
            return GetResultOrDefault(_lightBulb?.GetBrightnessAsync(), 0);
        }

        public Task<Color> GetColorAsync()
        {
            return GetResultOrDefault(_lightBulb?.GetColorAsync(), Color.FromArgb(0));
        }

        public Task<int> GetTemperatureAsync()
        {
            return GetResultOrDefault(_lightBulb?.GetTemperatureAsync(), 0);
        }

        private async Task<T> GetResultOrDefault<T>(Task<T> task, T defaultValue)
        {
            try
            {
                if (task == null)
                    return defaultValue;
                return await task;
            }
            catch (Exception ex) // could be a connection lost exception or something similar
            {
                Logger.Log(ex);
                return defaultValue;
            }
        }

        private async Task SyncState()
        {
            if (!IsConnected)
                return;

            var tasks = new List<Task>();

            if (_pendingState.Contains(nameof(_state.IsPoweredOn)))
                tasks.Add(_lightBulb.SetPowerAsync(_state.IsPoweredOn));
            if (_pendingState.Contains(nameof(_state.Color)))
                tasks.Add(_lightBulb.SetColorAsync(_state.Color));
            if (_pendingState.Contains(nameof(_state.Brightness)))
                tasks.Add(_lightBulb.SetBrightnessAsync(_state.Brightness));
            if (_pendingState.Contains(nameof(_state.Temperature)))
                tasks.Add(_lightBulb.SetTemperatureAsync(_state.Temperature));

            try
            {
                await Task.WhenAll(tasks);
                _pendingState.Clear();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void LightBulb_PowerStatusChanged(object sender, LightBulbPropertyChangedEventArgs<bool> e)
        {
            if (_state.IsPoweredOn == e.Value)
                return;

            _state.IsPoweredOn = e.Value;
            PowerStatusChanged?.Invoke(this, e);
        }

        private void LightBulb_BrightnessChanged(object sender, LightBulbPropertyChangedEventArgs<int> e)
        {
            if (_state.Brightness == e.Value)
                return;

            _state.Brightness = e.Value;
            BrightnessChanged?.Invoke(this, e);
        }

        private void LightBulb_TemperatureChanged(object sender, LightBulbPropertyChangedEventArgs<int> e)
        {
            if (_state.Temperature == e.Value)
                return;

            _state.Temperature = e.Value;
            TemperatureChanged?.Invoke(this, e);
        }

        private void LightBulb_ColorChanged(object sender, LightBulbPropertyChangedEventArgs<Color> e)
        {
            if (_state.Color == e.Value)
                return;

            _state.Color = e.Value;
            ColorChanged?.Invoke(this, e);
        }
    }
}

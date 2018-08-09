using DeviceControl.Core;
using DeviceControl.Core.LightBulbs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using YeelightAPI;

namespace DeviceControl.Plugin.Yeelight
{
    public sealed class YeelightBulb : ILightBulb
    {
        private readonly Device _device;

        internal YeelightBulb(Guid id, Device device)
        {
            Id = id;
            _device = device;
            _device.OnNotificationReceived += Device_OnNotificationReceived;
            _device.OnError += Device_OnError;
        }

        public void Dispose()
        {
            _device.OnNotificationReceived -= Device_OnNotificationReceived;
            _device.OnError -= Device_OnError;
            _device.Dispose();
        }

        public event EventHandler Connected;
        public event EventHandler<LightBulbPropertyChangedEventArgs<bool>> PowerStatusChanged;
        public event EventHandler<LightBulbPropertyChangedEventArgs<int>> BrightnessChanged;
        public event EventHandler<LightBulbPropertyChangedEventArgs<int>> TemperatureChanged;
        public event EventHandler<LightBulbPropertyChangedEventArgs<Color>> ColorChanged;

        public Guid Id { get; }
        public bool IsConnected => _device.IsConnected;

        public async Task ConnectAsync()
        {
            // the API will disconnect then reconnect so avoid this behaviour
            if (IsConnected)
                return;

            await _device.Connect();
            Connected?.Invoke(this, EventArgs.Empty);
        }

        public async Task<bool> GetPowerAsync()
        {
            // temp workaround for yeelight bugs.. need to try a few times to get a property
            string result = null;
            for (var i = 0; i < 3; i++)
            {
                result = await _device.GetProp(YeelightAPI.Models.PROPERTIES.power) as string;
                if (result == "on" || result == "off")
                    break;
                await Task.Delay(250);
            }
            return result == "on";
        }

        public async Task<int> GetBrightnessAsync()
        {
            return await TryGetInt(() => _device.GetProp(YeelightAPI.Models.PROPERTIES.bright)) ?? 100;
        }

        public async Task<Color> GetColorAsync()
        {
            var rgb = await TryGetInt(() => _device.GetProp(YeelightAPI.Models.PROPERTIES.rgb)) ?? 0;
            // use the implicit 255 to ignore the alpha values in the rest of the application (Color.FromArgb(r, g, b) will use 255 alpha)
            return Color.FromArgb(255, Color.FromArgb(rgb));
        }

        public async Task<int> GetTemperatureAsync()
        {
            return await TryGetInt(() => _device.GetProp(YeelightAPI.Models.PROPERTIES.ct)) ?? 4000;
        }

        public Task SetPowerAsync(bool power)
        {
            if (power)
                return _device.TurnOn(1000);
            return _device.TurnOff(1000);
        }

        public async Task SetBrightnessAsync(int brightness)
        {
            // the bulb will slightly flicker when setting to the same value, so prevent that from happening
            if (await GetBrightnessAsync() == brightness)
                return;

            await _device.SetBrightness(brightness, 1000);
        }

        public async Task SetColorAsync(Color color)
        {
            if (await GetColorAsync() == color)
                return;

            await _device.SetRGBColor(color.R, color.G, color.B, 1000);
        }

        public async Task SetTemperatureAsync(int temperature)
        {
            // throwing this for now to remind me
            if (temperature < 1000)
                throw new ArgumentException("Temperature cannot be less than 1000.", nameof(temperature));

            if (await GetTemperatureAsync() == temperature)
                return;

            await _device.SetColorTemperature(temperature, 1000);
        }

        private void Device_OnNotificationReceived(object sender, NotificationReceivedEventArgs e)
        {
            var result = e.Result;

            if (GetParamValue(result.Params, YeelightAPI.Models.PROPERTIES.power, out string powerStatus))
                PowerStatusChanged?.Invoke(this, new LightBulbPropertyChangedEventArgs<bool>(powerStatus == "on"));
            if (GetParamValue(result.Params, YeelightAPI.Models.PROPERTIES.bright, out decimal bright))
                BrightnessChanged?.Invoke(this, new LightBulbPropertyChangedEventArgs<int>((int)bright));
            if (GetParamValue(result.Params, YeelightAPI.Models.PROPERTIES.ct, out decimal temperature))
                TemperatureChanged?.Invoke(this, new LightBulbPropertyChangedEventArgs<int>((int)temperature));
            if (GetParamValue(result.Params, YeelightAPI.Models.PROPERTIES.rgb, out decimal color))
                ColorChanged?.Invoke(this, new LightBulbPropertyChangedEventArgs<Color>(Color.FromArgb((int)color)));
        }

        private async Task<int?> TryGetInt(Func<Task<object>> getValue)
        {
            // temp solution because of a bug in yeelight (it will give the wrong prop back)
            for (var i = 0; i < 3; i++)
            {
                var bright = (string) await getValue();
                if (int.TryParse(bright, out int result))
                    return result;
                await Task.Delay(250);
            }

            // give up...
            return null;
        }

        private bool GetParamValue<T>(Dictionary<YeelightAPI.Models.PROPERTIES, object> parameters, YeelightAPI.Models.PROPERTIES prop, out T value)
        {
            if (parameters.TryGetValue(prop, out object obj) && obj is T)
            {
                value = (T) obj;
                return true;
            }

            value = default(T);
            return false;
        }

        private void Device_OnError(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log(this, LogLevel.Error, "Device error.", e.ExceptionObject as Exception);
        }
    }
}

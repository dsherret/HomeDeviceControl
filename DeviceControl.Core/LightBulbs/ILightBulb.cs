using System;
using System.Drawing;
using System.Threading.Tasks;

namespace DeviceControl.Core.LightBulbs
{
    public class LightBulbPropertyChangedEventArgs<T> : EventArgs
    {
        public LightBulbPropertyChangedEventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }

    public interface ILightBulb : IDisposable
    {
        event EventHandler Connected;
        event EventHandler<LightBulbPropertyChangedEventArgs<bool>> PowerStatusChanged;
        event EventHandler<LightBulbPropertyChangedEventArgs<int>> BrightnessChanged;
        event EventHandler<LightBulbPropertyChangedEventArgs<int>> TemperatureChanged;
        event EventHandler<LightBulbPropertyChangedEventArgs<Color>> ColorChanged;

        Guid Id { get; }
        bool IsConnected { get; }

        Task ConnectAsync();
        Task SetPowerAsync(bool power);
        Task SetBrightnessAsync(int brightness);
        Task SetColorAsync(Color color);
        Task SetTemperatureAsync(int temperature);

        Task<bool> GetPowerAsync();
        Task<int> GetBrightnessAsync();
        Task<Color> GetColorAsync();
        Task<int> GetTemperatureAsync();
    }
}

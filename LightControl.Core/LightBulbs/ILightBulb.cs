using System;
using System.Drawing;
using System.Threading.Tasks;

namespace LightControl.Core.LightBulbs
{
    public interface ILightBulb : IDisposable
    {
        Guid Id { get; }
        bool IsConnected { get; }

        Task ConnectAsync();
        Task TurnOnAsync();
        Task TurnOffAsync();
        Task SetRGBColorAsync(Color color);
        Task SetColorTemperature(int temperature);

        Task<bool> GetIsPoweredOn();
    }
}

using System;
using System.Threading.Tasks;

namespace LightControl.Core.LightBulbs
{
    public interface ILightBulb : IDisposable
    {
        string Id { get; }
        Task ConnectAsync();
        Task TurnOnAsync();
        Task TurnOffAsync();
        Task SetRGBColorAsync(int r, int g, int b);
    }
}

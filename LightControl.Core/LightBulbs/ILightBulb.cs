using System;
using System.Threading.Tasks;

namespace LightControl.Core.LightBulbs
{
    public interface ILightBulb : IDisposable
    {
        Task Connect();
        Task TurnOn();
        Task TurnOff();
        Task SetRGBColor(int r, int g, int b);
    }
}

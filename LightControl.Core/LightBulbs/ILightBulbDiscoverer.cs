using System;

namespace LightControl.Core.LightBulbs
{
    public interface ILightBulbDiscoverer
    {
        event EventHandler<LightBulbEventArgs> Discovered;
    }
}

using System;

namespace DeviceControl.Core.LightBulbs
{
    public interface ILightBulbDiscoverer
    {
        event EventHandler<LightBulbEventArgs> Discovered;
    }
}

using System;

namespace HomeDeviceControl.Core.LightBulbs
{
    public interface ILightBulbDiscoverer
    {
        event EventHandler<LightBulbEventArgs> Discovered;
    }
}

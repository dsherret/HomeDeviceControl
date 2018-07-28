using System;

namespace LightControl.Core.LightBulbs
{
    public class LightBulbDiscoveredEventArgs : EventArgs
    {
        public LightBulbDiscoveredEventArgs(ILightBulb lightBulb)
        {
            LightBulb = lightBulb;
        }

        public ILightBulb LightBulb { get; }
    }

    public interface ILightBulbDiscoverer
    {
        event EventHandler<LightBulbDiscoveredEventArgs> Discovered;
    }
}

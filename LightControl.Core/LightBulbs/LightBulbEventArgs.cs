using System;

namespace LightControl.Core.LightBulbs
{
    public class LightBulbEventArgs : EventArgs
    {
        public LightBulbEventArgs(ILightBulb lightBulb)
        {
            LightBulb = lightBulb;
        }

        public ILightBulb LightBulb { get; }
    }
}

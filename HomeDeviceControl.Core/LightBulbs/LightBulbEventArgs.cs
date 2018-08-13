using HomeDeviceControl.Core.Common;

namespace HomeDeviceControl.Core.LightBulbs
{
    public class LightBulbEventArgs : ValueEventArgs<ILightBulb>
    {
        public LightBulbEventArgs(ILightBulb value) : base(value)
        {
        }
    }
}

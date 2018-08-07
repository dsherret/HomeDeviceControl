using DeviceControl.Core.Common;

namespace DeviceControl.Core.LightBulbs
{
    public class LightBulbEventArgs : ValueEventArgs<ILightBulb>
    {
        public LightBulbEventArgs(ILightBulb value) : base(value)
        {
        }
    }
}

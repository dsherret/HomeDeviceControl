using DeviceControl.Core.Common;

namespace DeviceControl.LightBulbs
{
    public class LightBulbWrapperEventArgs : ValueEventArgs<LightBulbWrapper>
    {
        public LightBulbWrapperEventArgs(LightBulbWrapper value) : base(value)
        {
        }
    }
}

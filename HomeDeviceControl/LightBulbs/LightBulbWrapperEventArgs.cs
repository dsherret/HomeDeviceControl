using HomeDeviceControl.Core.Common;

namespace HomeDeviceControl.LightBulbs
{
    public class LightBulbWrapperEventArgs : ValueEventArgs<LightBulbWrapper>
    {
        public LightBulbWrapperEventArgs(LightBulbWrapper value) : base(value)
        {
        }
    }
}

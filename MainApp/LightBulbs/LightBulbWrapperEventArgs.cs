using HomeDeviceControl.Core.Common;

namespace HomeDeviceControl.MainApp.LightBulbs
{
    public class LightBulbWrapperEventArgs : ValueEventArgs<LightBulbWrapper>
    {
        public LightBulbWrapperEventArgs(LightBulbWrapper value) : base(value)
        {
        }
    }
}

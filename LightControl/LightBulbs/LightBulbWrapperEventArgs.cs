using LightControl.Core.Common;

namespace LightControl.LightBulbs
{
    public class LightBulbWrapperEventArgs : ValueEventArgs<LightBulbWrapper>
    {
        public LightBulbWrapperEventArgs(LightBulbWrapper value) : base(value)
        {
        }
    }
}

using LightControl.Core.Common;

namespace LightControl.Core.LightBulbs
{
    public class LightBulbEventArgs : ValueEventArgs<ILightBulb>
    {
        public LightBulbEventArgs(ILightBulb value) : base(value)
        {
        }
    }
}

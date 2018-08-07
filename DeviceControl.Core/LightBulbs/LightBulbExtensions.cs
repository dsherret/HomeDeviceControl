using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DeviceControl.Core.LightBulbs
{
    public static class LightBulbExtensions
    {
        private class Count
        {
            public int Value { get; set; }
        }

        private static ConditionalWeakTable<ILightBulb, Count> _references = new ConditionalWeakTable<ILightBulb, Count>();
        private static object _lock = new object();

        public static Task IncrementOnAsync(this ILightBulb bulb)
        {
            var count = _references.GetOrCreateValue(bulb);
            lock (_lock)
            {
                count.Value++;
                if (count.Value == 1)
                    return bulb.SetPowerAsync(true);
                return Task.CompletedTask;
            }
        }

        public static Task<bool> IncrementOffAsync(this ILightBulb bulb)
        {
            var count = _references.GetOrCreateValue(bulb);
            lock (_lock)
            {
                count.Value--;
                if (count.Value == 0)
                    return bulb.SetPowerAsync(false).ContinueWith(_ => true);
                return Task.FromResult(false);
            }
        }
    }
}

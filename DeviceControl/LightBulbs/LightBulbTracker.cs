using System;
using System.Collections.Generic;
using System.Linq;

namespace DeviceControl.LightBulbs
{
    public class LightBulbTracker
    {
        private readonly object _lock = new object();
        private readonly HashSet<LightBulbWrapper> _trackedLightBulbs = new HashSet<LightBulbWrapper>();

        public event EventHandler<LightBulbWrapperEventArgs> BulbConnected;
        public event EventHandler<LightBulbWrapperEventArgs> BulbPowerChanged;

        public bool TrackLightBulb(LightBulbWrapper bulb)
        {
            lock (_lock)
            {
                if (_trackedLightBulbs.Contains(bulb))
                    return false;

                _trackedLightBulbs.Add(bulb);
                bulb.Connected += BulbConnected;
                bulb.PowerChanged += BulbPowerChanged;
                return true;
            }
        }

        public bool UntrackLightBulb(LightBulbWrapper bulb)
        {
            lock (_lock)
            {
                if (!_trackedLightBulbs.Remove(bulb))
                    return false;

                bulb.Connected -= BulbConnected;
                bulb.PowerChanged -= BulbPowerChanged;

                return true;
            }
        }

        public LightBulbWrapper[] GetTrackedLightBulbs()
        {
            lock (_lock)
                return _trackedLightBulbs.ToArray();
        }
    }
}

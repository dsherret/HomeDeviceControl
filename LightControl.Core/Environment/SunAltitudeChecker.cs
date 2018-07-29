using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LightControl.Core.Environment
{
    /// <summary>
    /// Periodically checks the sun's altitude.
    /// </summary>
    public class SunAltitudeChecker
    {
        private readonly SunCalculator _sunCalculator;
        private readonly TimeSpan _updateInterval;

        public class AltitudeInfoEventArgs : EventArgs
        {
            internal AltitudeInfoEventArgs(double altitude)
            {
                Altitude = altitude;
            }

            /// <summary>
            /// Altitude in degrees (ex. -6 is -6 degrees).
            /// </summary>
            public double Altitude { get; set; }
        }

        public SunAltitudeChecker(GeoLocation location, TimeSpan updateInterval)
        {
            _updateInterval = updateInterval;
            _sunCalculator = new SunCalculator(location);

            Task.Run(StartRunning);
        }

        private readonly EventHandlerList _events = new EventHandlerList();
        public event EventHandler<AltitudeInfoEventArgs> AltitudeChecked
        {
            add
            {
                _events.AddHandler(nameof(AltitudeChecked), value);
                // fire whenever someone subscribes
                FireAltitudeChecked();
            }
            remove
            {
                _events.RemoveHandler(nameof(AltitudeChecked), value);
            }
        }

        private async Task StartRunning()
        {
            while(true)
            {
                FireAltitudeChecked();
                // doesn't need to be extremely accurate, so just wait the interval
                await Task.Delay(_updateInterval);
            }
        }

        private void FireAltitudeChecked()
        {
            var sunAltitude = _sunCalculator.GetSunAltitude(DateTime.Now);
            ((EventHandler<AltitudeInfoEventArgs>)_events[nameof(AltitudeChecked)])?.Invoke(this, new AltitudeInfoEventArgs(sunAltitude));
        }
    }
}

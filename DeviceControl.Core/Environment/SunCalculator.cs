using CoordinateSharp;
using System;

namespace DeviceControl.Core.Environment
{
    public class SunCalculator
    {
        private readonly Func<GeoLocation> _location;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="location">Location to get the sun information for.</param>
        public SunCalculator(Func<GeoLocation> location)
        {
            _location = location;
        }

        /// <summary>
        /// Gets the sun altitude in degrees for a specified time.
        /// </summary>
        /// <param name="dateTime">The current date time.</param>
        public double GetSunAltitude(DateTime dateTime)
        {
            var location = _location();
            var coordinate = new Coordinate(location.Latitute, location.Longitude, dateTime.ToUniversalTime());
            return coordinate.CelestialInfo.SunAltitude;
        }
    }
}

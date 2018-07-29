using CoordinateSharp;
using System;

namespace LightControl.Core.Environment
{
    public class SunCalculator
    {
        private readonly GeoLocation _location;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="location">Location to get the sun information for.</param>
        public SunCalculator(GeoLocation location)
        {
            _location = location;
        }

        /// <summary>
        /// Gets the sun altitude in degrees for a specified time.
        /// </summary>
        /// <param name="dateTime">The current date time.</param>
        public double GetSunAltitude(DateTime dateTime)
        {
            var coordinate = new Coordinate(_location.Latitute, _location.Longitude, dateTime.ToUniversalTime());
            return coordinate.CelestialInfo.SunAltitude;
        }
    }
}

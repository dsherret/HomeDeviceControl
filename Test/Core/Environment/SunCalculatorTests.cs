using HomeDeviceControl.Core.Environment;
using System;
using Xunit;

namespace HomeDeviceControl.Test.Core.Environment
{
    public class SunCalculatorTest
    {
        private readonly SunCalculator _sunCalculator = new SunCalculator(() => new GeoLocation
        {
            Latitute = 43.653908,
            Longitude = -79.384293
        });

        [Fact]
        public void GetSunAltitude_Noon_AboveZero()
        {
            Assert.True(_sunCalculator.GetSunAltitude(new DateTime(2000, 1, 1, 12, 0, 0)) > 0);
        }

        [Fact]
        public void GetSunAltitude_Night_BelowZero()
        {
            Assert.True(_sunCalculator.GetSunAltitude(new DateTime(2000, 1, 1, 22, 0, 0)) < 0);
        }
    }
}

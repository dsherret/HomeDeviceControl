using HomeDeviceControl.Core.Utils;
using System;
using Xunit;

namespace HomeDeviceControl.Test.Core.Utils
{
    public class TimeRangeCalculatorTests
    {
        [Theory]
        [InlineData("10:00:00", 10, 20, "10:00:00", "10:10:00", 10)]
        [InlineData("10:10:00", 10, 20, "10:00:00", "10:10:00", 20)]
        [InlineData("10:05:00", 10, 20, "10:00:00", "10:10:00", 15)]
        [InlineData("23:55:00", 10, 20, "23:50:00", "00:10:00", 12.5)]
        [InlineData("00:05:00", 10, 20, "23:50:00", "00:10:00", 17.5)]
        public void GetValueTests(string currentTime, double minValue, double maxValue, string minTime, string maxTime, double expected)
        {
            Assert.Equal(expected, TimeRangeCalculator.GetValue(
                StringToTimeSpan(currentTime),
                minValue,
                maxValue,
                StringToTimeSpan(minTime),
                StringToTimeSpan(maxTime)));
        }

        [Theory]
        [InlineData("09:59:59", 10, 20, "10:00:00", "10:10:00")]
        [InlineData("10:10:01", 10, 20, "10:00:00", "10:10:00")]
        [InlineData("23:30:00", 10, 20, "23:50:00", "00:10:00")]
        [InlineData("00:20:00", 10, 20, "23:50:00", "00:10:00")]
        [InlineData("04:30:00", 10, 20, "23:50:00", "00:10:00")]
        public void GetValue_ThrowsTests(string currentTime, double minValue, double maxValue, string minTime, string maxTime)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TimeRangeCalculator.GetValue(
                StringToTimeSpan(currentTime),
                minValue,
                maxValue,
                StringToTimeSpan(minTime),
                StringToTimeSpan(maxTime)));
        }

        private static TimeSpan StringToTimeSpan(string str)
        {
            var items = str.Split(":");
            return new TimeSpan(int.Parse(items[0]), int.Parse(items[1]), int.Parse(items[2]));
        }
    }
}

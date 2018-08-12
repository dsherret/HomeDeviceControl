using System;

namespace DeviceControl.Core.Utils
{
    public class TimeRangeCalculator
    {
        /// <summary>
        /// Provides a value between a given min and max value based on the current time.
        /// </summary>
        /// <param name="currentTime">Current time. Must be between the min and max time.</param>
        /// <param name="minValue">Value at the minimum time.</param>
        /// <param name="maxValue">Value at the maximum time.</param>
        /// <param name="minTime">Minimum possible time.</param>
        /// <param name="maxTime">Maximum possible time.</param>
        /// <returns>The linear progress value.</returns>
        public static double GetValue(TimeSpan currentTime, double minValue, double maxValue, TimeSpan minTime, TimeSpan maxTime)
        {
            var timePercent = GetTimePercent(currentTime, minTime, maxTime);
            var valueRange = maxValue - minValue;

            return valueRange * timePercent + minValue;
        }

        private static double GetTimePercent(TimeSpan currentTime, TimeSpan minTime, TimeSpan maxTime)
        {
            // shift everything
            if (minTime > maxTime)
            {
                maxTime = maxTime.Add(TimeSpan.FromDays(1));
                if (currentTime < minTime && currentTime.Add(TimeSpan.FromDays(1)) < maxTime)
                    currentTime = currentTime.Add(TimeSpan.FromDays(1));
            }

            if (currentTime < minTime || currentTime > maxTime)
                throw new ArgumentOutOfRangeException(nameof(currentTime), $"Current time {currentTime} is out of range {minTime} - {maxTime}.");

            var timeRange = maxTime - minTime;
            var timeProgress = currentTime - minTime;

            return timeProgress.Ticks / timeRange.Ticks;
        }
    }
}

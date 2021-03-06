﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using HomeDeviceControl.Core;

namespace HomeDeviceControl.MainApp.LightBulbs
{
    public sealed class LightBulbColorController : IDisposable
    {
        private readonly LightBulbTracker _lightBulbTracker = new LightBulbTracker();
        private readonly HomeStateContainer _homeStateContainer;

        public LightBulbColorController(HomeStateContainer homeStateContainer)
        {
            _homeStateContainer = homeStateContainer;
            homeStateContainer.OnStateUpdated(_ => UpdateLightBulbsAsync());

            _lightBulbTracker.BulbConnected += LightBulb_Connected;
            _lightBulbTracker.BulbPowerChanged += LightBulb_Connected;
        }

        public void Dispose()
        {
            _lightBulbTracker.BulbConnected -= LightBulb_Connected;
            _lightBulbTracker.BulbPowerChanged -= LightBulb_Connected;
        }

        public async void HandleLightBulb(LightBulbWrapper bulb)
        {
            if (_lightBulbTracker.TrackLightBulb(bulb))
                Logger.Log(this, LogLevel.Info, $"Handling color of bulb: {bulb.Id}");

            await UpdateLightBulbAsync(bulb);
        }

        public void UnhandleLightBulb(LightBulbWrapper bulb)
        {
            if (_lightBulbTracker.UntrackLightBulb(bulb))
                Logger.Log(this, LogLevel.Info, $"Unhandling color of bulb: {bulb.Id}");
        }

        private async void LightBulb_Connected(object sender, LightBulbWrapperEventArgs e)
        {
            await UpdateLightBulbAsync(e.Value);
        }

        private async void LightBulb_PowerChanged(object sender, LightBulbWrapperEventArgs e)
        {
            await UpdateLightBulbAsync(e.Value);
        }

        private async Task UpdateLightBulbsAsync()
        {
            var lightBulbs = _lightBulbTracker.GetTrackedLightBulbs();
            foreach (var lightBulb in lightBulbs)
                await UpdateLightBulbAsync(lightBulb);
        }

        private async Task UpdateLightBulbAsync(LightBulbWrapper bulb)
        {
            if (!(await bulb.GetPowerAsync()))
                return;

            var state = _homeStateContainer.GetCurrentState();
            var color = GetColorForState(state);
            if (color.HasValue)
                await bulb.SetColorAsync(color.Value);
            else
                await bulb.SetTemperatureAsync(GetTemperatureForState(state));
        }

        private static Color? GetColorForState(HomeState state)
        {
            if (state.CurrentTime.TimeOfDay >= new TimeSpan(23, 0, 0) || state.CurrentTime.TimeOfDay < new TimeSpan(5, 0, 0))
                return Color.FromArgb(255, 1, 1);
            return null;
        }

        private static int GetTemperatureForState(HomeState state)
        {
            // todo: clean up this code... this was quick and dirty
            const double MAX_ALTITUDE = 5;
            const double MIN_ALTITUDE = -12;
            const int MAX_TEMP = 4500;
            const int MIN_TEMP = 2500;
            const int TEMP_RANGE = MAX_TEMP - MIN_TEMP;

            if (state.CurrentTime.TimeOfDay >= new TimeSpan(22, 30, 0) || state.CurrentTime.TimeOfDay < new TimeSpan(5, 0, 0))
                return 2000;
            if (state.SunAltitude > MAX_ALTITUDE)
                return MAX_TEMP;

            return Math.Max(GetTemperatureForTime(state.CurrentTime.TimeOfDay), GetTemperatureForAltitude(state.SunAltitude));

            int GetTemperatureForTime(TimeSpan timeOfDay)
            {
                var minTimeOfDay = new TimeSpan(15, 0, 0);
                var maxTimeOfDay = new TimeSpan(22, 30, 0);
                if (timeOfDay <= minTimeOfDay || timeOfDay >= maxTimeOfDay)
                    return MIN_TEMP; // this is very dependent on the Math.Max above

                var percent = 1 - (timeOfDay - minTimeOfDay) / (maxTimeOfDay - minTimeOfDay);
                return (int)(TEMP_RANGE * percent + MIN_TEMP);
            }

            int GetTemperatureForAltitude(double altitude)
            {
                if (altitude > MAX_ALTITUDE)
                    return MAX_TEMP;
                if (altitude < MIN_ALTITUDE)
                    return MIN_TEMP;

                var altitudeRange = MAX_ALTITUDE - MIN_ALTITUDE;
                var percent = 1 - (altitude - MIN_ALTITUDE) / altitudeRange;
                return (int)(TEMP_RANGE * percent + MIN_TEMP);
            }
        }
    }
}

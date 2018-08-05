﻿using LightControl.Core.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using LightControl.Core;

namespace LightControl.LightBulbs
{
    public class LightBulbColorController
    {
        private readonly object _lock = new object();
        private readonly HashSet<LightBulbWrapper> _trackedLightBulbs = new HashSet<LightBulbWrapper>();
        private readonly HomeStateContainer _homeStateContainer;

        public LightBulbColorController(HomeStateContainer homeStateContainer)
        {
            _homeStateContainer = homeStateContainer;

            homeStateContainer.OnStateUpdated(_ => UpdateLightBulbsAsync());
        }

        public async void HandleLightBulb(LightBulbWrapper bulb)
        {
            lock (_lock)
            {
                if (!_trackedLightBulbs.Contains(bulb))
                {
                    _trackedLightBulbs.Add(bulb);
                    bulb.Connected += LightBulb_Connected;
                    bulb.PowerChanged += LightBulb_PowerChanged;
                    Logger.Log(this, LogLevel.Info, $"Handling color of bulb: {bulb.Id}");
                }
            }
            await UpdateLightBulbAsync(bulb);
        }

        public void UnhandleLightBulb(LightBulbWrapper bulb)
        {
            lock (_lock)
            {
                _trackedLightBulbs.Remove(bulb);
                bulb.Connected -= LightBulb_Connected;
                bulb.PowerChanged -= LightBulb_PowerChanged;
                Logger.Log(this, LogLevel.Info, $"Unhandling color of bulb: {bulb.Id}");
            }
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
            var lightBulbs = GetTrackedLightBulbs();
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
            await bulb.SetBrightnessAsync(GetBrightnessForState(state));
        }

        private LightBulbWrapper[] GetTrackedLightBulbs()
        {
            lock (_lock)
                return _trackedLightBulbs.ToArray();
        }

        private static int GetBrightnessForState(HomeState state)
        {
            if (state.CurrentTime.TimeOfDay >= new TimeSpan(23, 0, 0) || state.CurrentTime.TimeOfDay < new TimeSpan(0, 30, 0))
                return 50;
            if (state.CurrentTime.TimeOfDay >= new TimeSpan(0, 30, 0) || state.CurrentTime.TimeOfDay < new TimeSpan(5, 0, 0))
                return 10;
            return 100;
        }

        private static Color? GetColorForState(HomeState state)
        {
            if (state.CurrentTime.TimeOfDay >= new TimeSpan(23, 0, 0) || state.CurrentTime.TimeOfDay < new TimeSpan(5, 0, 0))
                return Color.FromArgb(255, 1, 1);
            return null;
        }

        private static int GetTemperatureForState(HomeState state)
        {
            if (state.CurrentTime.TimeOfDay >= new TimeSpan(22, 30, 0) || state.CurrentTime.TimeOfDay < new TimeSpan(6, 0, 0))
                return 2000;
            return GetTemperatureForAltitude(state.SunAltitude);
        }

        private static int GetTemperatureForAltitude(double altitude)
        {
            const double MAX_ALTITUDE = 5;
            const double MIN_ALTITUDE = -12;
            const int MAX_TEMP = 4500;
            const int MIN_TEMP = 2500;
            if (altitude > MAX_ALTITUDE)
                return MAX_TEMP;
            if (altitude < MIN_ALTITUDE)
                return MIN_TEMP;

            var altitudeRange = MAX_ALTITUDE - MIN_ALTITUDE;
            var percent = (altitude - MIN_ALTITUDE) / altitudeRange;

            var tempRange = MAX_TEMP - MIN_TEMP;
            return (int)(tempRange * percent + MIN_TEMP);
        }
    }
}

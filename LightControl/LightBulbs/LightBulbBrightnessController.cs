using LightControl.Core;
using System;
using System.Threading.Tasks;

namespace LightControl.LightBulbs
{
    public sealed class LightBulbBrightnessController : IDisposable
    {
        private readonly LightBulbTracker _lightBulbTracker = new LightBulbTracker();
        private readonly HomeStateContainer _homeStateContainer;

        public LightBulbBrightnessController(HomeStateContainer homeStateContainer)
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
                Logger.Log(this, LogLevel.Info, $"Handling brightness of bulb: {bulb.Id}");

            await UpdateLightBulbAsync(bulb);
        }

        public void UnhandleLightBulb(LightBulbWrapper bulb)
        {
            if (_lightBulbTracker.UntrackLightBulb(bulb))
                Logger.Log(this, LogLevel.Info, $"Unhandling brightness of bulb: {bulb.Id}");
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
            await bulb.SetBrightnessAsync(GetBrightnessForState(state));
        }

        private static int GetBrightnessForState(HomeState state)
        {
            if (state.CurrentTime.TimeOfDay >= new TimeSpan(23, 0, 0) || state.CurrentTime.TimeOfDay < new TimeSpan(0, 30, 0))
                return 50;
            if (state.CurrentTime.TimeOfDay >= new TimeSpan(0, 30, 0) && state.CurrentTime.TimeOfDay < new TimeSpan(5, 0, 0))
                return 10;
            return 100;
        }
    }
}

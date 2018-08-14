using HomeDeviceControl.Communication.Server;
using HomeDeviceControl.Core.Environment;
using HomeDeviceControl.MainApp.Config;
using System;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace HomeDeviceControl.MainApp
{
    public sealed class MainAppRunner : IDisposable
    {
        private readonly HomeContext _homeContext = new HomeContext();
        private readonly Server _server;
        private readonly Timer _timer = new Timer(Settings.Default.UpdateIntervalSeconds * 1000);

        public MainAppRunner()
        {
            _server = ServerConfig.Setup(_homeContext);
            SunroomConfig.Setup(_homeContext);
        }

        public void Dispose()
        {
            _server.Dispose();
            _homeContext.Dispose();
            _timer.Dispose();
        }

        public async Task StartAsync()
        {
            await _server.RunAsync();
            StartUpdatingState();
        }

        private void StartUpdatingState()
        {
            // hook up stuff that could change
            var sunCalculator = new SunCalculator(() => new GeoLocation
            {
                Latitute = Settings.Default.Latitude,
                Longitude = Settings.Default.Longitude
            });

            _timer.Elapsed += (sender, e) =>
            {
                UpdateState();
            };
            _timer.Start();

            UpdateState();

            void UpdateState()
            {
                _homeContext.HomeStateContainer.UpdateState(state =>
                {
                    state.SunAltitude = sunCalculator.GetSunAltitude(DateTime.Now);
                    state.CurrentTime = DateTime.Now;
                });
            }
        }
    }
}

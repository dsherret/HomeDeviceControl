using HomeDeviceControl.Communication.Server;
using HomeDeviceControl.Core;
using HomeDeviceControl.Core.Environment;
using HomeDeviceControl.MainApp.Config;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeDeviceControl.MainApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Logger.Configure("log4net.config");

            var homeContext = new HomeContext();
            var server = await RunServerAsync(homeContext);

            SunroomConfig.Setup(homeContext);

            // hook up stuff that could change
            var sunCalculator = new SunCalculator(() => new GeoLocation
            {
                Latitute = Settings.Default.Latitude,
                Longitude = Settings.Default.Longitude
            });
            var timer = new System.Timers.Timer();
            timer.Interval = 30_000; // todo: move to settings file
            timer.Elapsed += (sender, e) =>
            {
                UpdateState();
            };
            timer.Start();
            UpdateState();

            new ManualResetEvent(false).WaitOne();

            server.Dispose();

            void UpdateState()
            {
                homeContext.HomeStateContainer.UpdateState(state =>
                {
                    state.SunAltitude = sunCalculator.GetSunAltitude(DateTime.Now);
                    state.CurrentTime = DateTime.Now;
                });
            }
        }

        private static async Task<Server> RunServerAsync(HomeContext homeContext)
        {
            var server = new Server(Settings.Default.ServerHostname, Settings.Default.ServerPort);
            server.AddValueListener(homeContext.DevicePowerStatusReceiver);
            await server.RunAsync();
            return server;
        }
    }
}

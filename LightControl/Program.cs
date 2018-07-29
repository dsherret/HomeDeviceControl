using LightControl.Communication.Common;
using LightControl.Communication.Server;
using LightControl.Core;
using LightControl.Core.Environment;
using LightControl.LightBulbs;
using LightControl.Config;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LightControl
{
    class Program
    {
        private static readonly DevicePowerStatusReceiver _devicePowerStatusReceiver = new DevicePowerStatusReceiver(Routes.DevicePowerStatus);
        private static LightBulbFactory _lightBulbFactory;
        private static Server _server;

        static async Task Main(string[] args)
        {
            var homeState = new HomeStateContainer();
            RunServer();

            var pluginSystem = new PluginSystem();
            _lightBulbFactory = new LightBulbFactory(pluginSystem.LightBulbStore);

            SunRoomConfig.Setup(homeState, _lightBulbFactory, _devicePowerStatusReceiver);

            // hook up stuff that could change
            var sunCalculator = new SunCalculator(new GeoLocation
            {
                Latitute = 43.653908,
                Longitude = -79.384293
            });
            var timer = new System.Timers.Timer();
            timer.Interval = 5_000;
            timer.Elapsed += (sender, e) =>
            {
                homeState.UpdateState(state =>
                {
                    state.SunAltitude = sunCalculator.GetSunAltitude(DateTime.Now);
                    state.CurrentTime = DateTime.Now;
                });
            };
            timer.Start();

            // handle changing temperature
            homeState.StateUpdated += async (sender, e) =>
            {
                /*var temperature = GetTemperatureForState(e.NewState);
                var tasks = new List<Task>();

                foreach (var lightBulb in lightBulbStore.GetAll())
                    tasks.Add(lightBulb.SetTemperatureAsync(temperature));

                await Task.WhenAll(tasks);*/
            };

            //foreach (var lightBulb in lightBulbs)
            //{
            //await lightBulb.Connect();
            //await lightBulb.SetRGBColor(200, 100, 0);
            //}


            new ManualResetEvent(false).WaitOne();
            Console.WriteLine("FINI");
            Console.ReadKey();

            _server.Dispose();
        }

        private static async void RunServer()
        {
            _server = new Server(8084);
            _server.AddValueListener(_devicePowerStatusReceiver);
            await _server.RunAsync();
        }

        private static int GetTemperatureForState(HomeState newState)
        {
            if (newState.CurrentTime.TimeOfDay > new TimeSpan(23, 0, 0) || newState.CurrentTime.TimeOfDay < new TimeSpan(6, 0, 0))
                return 2000;
            return GetTemperatureForAltitude(newState.SunAltitude);
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

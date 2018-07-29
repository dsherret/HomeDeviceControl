using LightControl.Communication.Common;
using LightControl.Communication.Server;
using LightControl.Core;
using LightControl.Core.Environment;
using LightControl.Core.LightBulbs;
using LightControl.Plugin.ZoozSensor;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LightControl
{
    class Program
    {
        private static readonly DevicePowerStatusReceiver _devicePowerStatusReceiver = new DevicePowerStatusReceiver(Routes.DevicePowerStatus);
        private static Server _server;

        static async Task Main(string[] args)
        {
            var homeState = new HomeStateContainer();
            RunServer();

            var pluginSystem = new PluginSystem();
            var lightBulbStore = pluginSystem.LightBulbStore;
            var sunRoomBulb = lightBulbStore.Get(DeviceIdentifiers.SunroomLightId);
            var motionSensor = new ZoozMotionSensor(5);
            var lightSensor = new ZoozLightSensor(5);

            // hook up stuff that could change
            var sunAltitudeChecker = new SunAltitudeChecker(new GeoLocation
            {
                Latitute = 43.653908,
                Longitude = -79.384293
            }, TimeSpan.FromMinutes(1));
            sunAltitudeChecker.AltitudeChecked += (sender, e) =>
            {
                homeState.UpdateState(state =>
                {
                    state.SunAltitude = e.Altitude;
                    return state;
                });
            };

            _devicePowerStatusReceiver.PowerStatusChanged += (sender, e) =>
            {
                if (e.DevicePowerStatus.DeviceId == DeviceIdentifiers.ComputerId)
                {
                    homeState.UpdateState(state =>
                    {
                        state.IsComputerOn = e.DevicePowerStatus.IsPoweredOn;
                        return state;
                    });
                }
            };

            // handle changing temperature
            homeState.StateUpdated += async (sender, e) =>
            {
                var temperature = GetTemperatureForAltitude(e.NewState.SunAltitude);
                var tasks = new List<Task>();

                foreach (var lightBulb in lightBulbStore.GetAll())
                    tasks.Add(lightBulb.SetColorTemperature(temperature));

                await Task.WhenAll(tasks);
            };

            // handle turning on sunroom light when computer turns on or off
            homeState.StateUpdated += async (sender, e) =>
            {
                if (e.PreviousState.IsComputerOn != e.NewState.IsComputerOn)
                    await sunRoomBulb.ToggleOnAsync(e.NewState.IsComputerOn);
            };

            //foreach (var lightBulb in lightBulbs)
            //{
            //await lightBulb.Connect();
            //await lightBulb.SetRGBColor(200, 100, 0);
            //}

            lightSensor.LuminanceChanged += (sender, a) =>
            {
                Console.WriteLine("Luminance: " + a.Value);
            };

            motionSensor.MotionDetected += async (sender, a) =>
            {
                Console.WriteLine("Detected motion");
                /*await sunRoomBulb.IncrementOnAsync();
                await Task.Delay(15000);
                var result = await sunRoomBulb.IncrementOffAsync();
                if (result)
                    Console.WriteLine("OFF");*/
            };

            new ManualResetEvent(false).WaitOne();
            Console.WriteLine("FINI");
            Console.ReadKey();

            lightSensor.Dispose();
            motionSensor.Dispose();
            _server.Dispose();
        }

        private static void DevicePowerStatusReceiver_PowerStatusChanged(object sender, DevicePowerStatusReceiver.DevicePowerStatusChangedEventArgs args)
        {
        }

        private static async void RunServer()
        {
            _server = new Server(8084);
            _server.AddValueListener(_devicePowerStatusReceiver);
            await _server.RunAsync();
        }

        private static int GetTemperatureForAltitude(double altitude)
        {
            const double MAX_ALTITUDE = 5;
            const double MIN_ALTITUDE = -12;
            const int MAX_TEMP = 4000;
            const int MIN_TEMP = 2000;
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

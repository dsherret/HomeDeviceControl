using LightControl.Communication.Common;
using LightControl.Communication.Server;
using LightControl.Core;
using LightControl.Core.LightBulbs;
using LightControl.Core.Sensors;
using System;
using System.Linq;
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
            const string SUNROOM_LIGHT_ID = "0x0000000005e0eaf1";

            _devicePowerStatusReceiver.PowerStatusChanged += DevicePowerStatusReceiver_PowerStatusChanged;

            RunServer();

            var pluginSystem = new PluginSystem();
            var lightBulbs = (await pluginSystem.GetLightBulbs()).ToArray();
            var sensors = (await pluginSystem.GetSensors()).ToArray();

            var sunRoomBulb = lightBulbs.First(b => b.Id == SUNROOM_LIGHT_ID);

            //foreach (var lightBulb in lightBulbs)
            //{
            //await lightBulb.Connect();
            //await lightBulb.SetRGBColor(200, 100, 0);
            //}

            sensors.OfType<ILightSensor>().First().LuminanceChanged += (sender, a) =>
            {
                Console.WriteLine("Luminance: " + a.Value);
            };

            sensors.OfType<IMotionSensor>().First().MotionDetected += async (sender, a) =>
            {
                Console.WriteLine("MOTION DETECTED");
                await sunRoomBulb.IncrementOnAsync();
                await Task.Delay(15000);
                var result = await sunRoomBulb.IncrementOffAsync();
                if (result)
                    Console.WriteLine("OFF");
            };

            new ManualResetEvent(false).WaitOne();
            Console.WriteLine("FINI");
            foreach (var sensor in sensors)
            {
                //sensor.Dispose();
            }
            Console.ReadKey();

            _server.Dispose();
        }

        private static void DevicePowerStatusReceiver_PowerStatusChanged(object sender, DevicePowerStatusReceiver.DevicePowerStatusChangedEventArgs args)
        {
            switch (args.DevicePowerStatus.DeviceId)
            {
                //case DeviceIdentifiers.ComputerId:
            }
        }

        static async void RunServer()
        {
            _server = new Server(8084);
            _server.AddValueListener(_devicePowerStatusReceiver);
            await _server.RunAsync();
        }
    }
}

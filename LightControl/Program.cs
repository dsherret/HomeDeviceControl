using LightControl.Communication.Server;
using LightControl.Computer;
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
        static async Task Main(string[] args)
        {
            const string SUNROOM_LIGHT_ID = "0x0000000005e0eaf1";
            var pluginSystem = new PluginSystem();
            var lightBulbs = (await pluginSystem.GetLightBulbs()).ToArray();
            var sensors = (await pluginSystem.GetSensors()).ToArray();

            var sunRoomBulb = lightBulbs.First(b => b.Id == SUNROOM_LIGHT_ID);
            var powerStatusReceiver = new PowerStatusReceiver("/computer/power-status");
            var computerStatus = new ComputerStatus(powerStatusReceiver, "DESKTOP-U8QP2IN");

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
        }
    }
}

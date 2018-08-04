using LightControl.Communication.Common;
using LightControl.Communication.Server;
using LightControl.Core;
using LightControl.Core.Environment;
using LightControl.LightBulbs;
using LightControl.Config;
using System;
using System.Threading;
using LightControl.Core.Utils;
using System.Threading.Tasks;

namespace LightControl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var homeContext = new HomeContext();
            var server = await RunServerAsync(homeContext);

            SunRoomConfig.Setup(homeContext);

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
                homeContext.HomeStateContainer.UpdateState(state =>
                {
                    state.SunAltitude = sunCalculator.GetSunAltitude(DateTime.Now);
                    state.CurrentTime = DateTime.Now;
                });
            };
            timer.Start();

            new ManualResetEvent(false).WaitOne();
            Console.WriteLine("FINI");
            Console.ReadKey();

            server.Dispose();
        }

        private static async Task<Server> RunServerAsync(HomeContext homeContext)
        {
            var server = new Server(8084);
            server.AddValueListener(homeContext.DevicePowerStatusReceiver);
            await server.RunAsync();
            return server;
        }
    }
}

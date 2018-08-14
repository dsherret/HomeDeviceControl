using HomeDeviceControl.Core;
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

            var exitEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                exitEvent.Set();
            };

            using (var appRunner = new MainAppRunner())
            {
                await appRunner.StartAsync();

                exitEvent.WaitOne();
            }
        }
    }
}

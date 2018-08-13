using HomeDeviceControl.Core;
using System;
using System.ServiceProcess;

namespace HomeDeviceControl.WindowsEventService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args = null)
        {
            Logger.Configure("log4net.config");

            var eventService = new EventService();
#if DEBUG
            eventService.TestStart(args ?? new string[0]);
            Console.ReadLine();
            eventService.TestStop();
#else
            ServiceBase.Run(new [] { eventService });
#endif
        }
    }
}

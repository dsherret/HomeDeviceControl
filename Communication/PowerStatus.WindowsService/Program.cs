using HomeDeviceControl.Core;
using System;
using System.ServiceProcess;

namespace HomeDeviceControl.Communication.PowerStatus.WindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Logger.Configure("log4net.config");

            var eventService = new EventService();
#if DEBUG
            eventService.TestStart();
            Console.ReadLine();
            eventService.TestStop();
#else
            ServiceBase.Run(new [] { eventService });
#endif
        }
    }
}

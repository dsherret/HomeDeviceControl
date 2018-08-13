using System;
using System.ServiceProcess;

namespace DeviceControl.WindowsEventService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var eventService = new EventService();
#if DEBUG
            eventService.TestStart(new string[0]);
            Console.ReadLine();
            eventService.TestStop();
#else
            ServiceBase.Run(new [] { eventService });
#endif
        }
    }
}

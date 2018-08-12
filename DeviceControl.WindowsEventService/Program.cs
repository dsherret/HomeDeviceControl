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
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new EventService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

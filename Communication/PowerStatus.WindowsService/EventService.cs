using HomeDeviceControl.Core;
using System.ServiceProcess;

namespace HomeDeviceControl.Communication.PowerStatus.WindowsService
{
    public partial class EventService : ServiceBase
    {
        public EventService()
        {
            InitializeComponent();
        }

        internal void TestStart()
        {
            OnStart(new string[0]);
        }

        internal void TestStop()
        {
            OnStop();
        }

        protected async override void OnStart(string[] args)
        {
            Logger.Log(this, LogLevel.Info, "Service started.");
            await PowerStatus.SendAsync(true);
        }

        protected async override void OnStop()
        {
            await PowerStatus.SendAsync(false);
            Logger.Log(this, LogLevel.Info, "Service stopped.");
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            Logger.Log(this, LogLevel.Info, $"Received power event: {powerStatus}");

            switch (powerStatus)
            {
                case PowerBroadcastStatus.Suspend:
                    PowerStatus.SendAsync(false).Wait();
                    break;
                case PowerBroadcastStatus.ResumeSuspend:
                    PowerStatus.SendAsync(true).Wait();
                    break;
            }

            return base.OnPowerEvent(powerStatus);
        }
    }
}

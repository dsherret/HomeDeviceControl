using HomeDeviceControl.Core;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace HomeDeviceControl.Communication.WindowsEventService
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
            Logger.Log(this, LogLevel.Info, "Service starting...");
            await PowerStatus.SendAsync(true);
            new Thread(RunMessagePump).Start();
            Logger.Log(this, LogLevel.Info, "Service started.");
        }

        protected override void OnStop()
        {
            Logger.Log(this, LogLevel.Info, "Service stopped.");
            Application.Exit();
        }

        private void RunMessagePump()
        {
            Application.Run(new HiddenForm());
        }
    }
}

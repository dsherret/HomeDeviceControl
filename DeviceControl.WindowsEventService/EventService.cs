using DeviceControl.Core;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace DeviceControl.WindowsEventService
{
    public partial class EventService : ServiceBase
    {
        public EventService()
        {
            InitializeComponent();
        }

        internal void TestStart(string[] args)
        {
            OnStart(args);
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

        private void RunMessagePump()
        {
            Application.Run(new HiddenForm());
        }

        protected override void OnStop()
        {
            Logger.Log(this, LogLevel.Info, "Service stopped.");
            Application.Exit();
        }
    }
}

using LightControl.Core.Sensors;
using LightControl.ZWave;
using System;
using System.Threading.Tasks;
using ZWave;
using ZWave.CommandClasses;

namespace LightControl.Plugin.ZoozSensor
{
    public class ZoozMotionSensor : IMotionSensor
    {
        private readonly ZWaveController _controller;
        private Basic _basicCommand;

        public ZoozMotionSensor()
        {
            _controller = ZWaveControllerFactory.Instance.AcquireController();
        }

        public void Dispose()
        {
            _basicCommand.Changed -= BasicCommand_Changed;
            _controller.Close();
        }

        public event EventHandler MotionDetected;

        public async Task SetupAsync()
        {
            if (_basicCommand != null)
                return;

            var nodes = await _controller.GetNodes();
            var node = nodes[5];
            _basicCommand = node.GetCommandClass<Basic>();
            _basicCommand.Changed += BasicCommand_Changed;
        }

        private void BasicCommand_Changed(object sender, ReportEventArgs<BasicReport> args)
        {
            if (args.Report.Value == 255)
                MotionDetected?.Invoke(this, EventArgs.Empty);
        }
    }
}

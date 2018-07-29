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

        public ZoozMotionSensor(int nodeId)
        {
            _controller = ZWaveControllerFactory.Instance.AcquireController();
            SetupAsync(nodeId);
        }

        public void Dispose()
        {
            if (_basicCommand != null)
                _basicCommand.Changed -= BasicCommand_Changed;
            ZWaveControllerFactory.Instance.ReleaseController();
        }

        public event EventHandler MotionDetected;

        private async void SetupAsync(int nodeId)
        {
            if (_basicCommand != null)
                return;

            var nodes = await _controller.GetNodes();
            var node = nodes[(byte)nodeId];

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

using LightControl.Core;
using LightControl.Core.Sensors;
using LightControl.Core.Utils;
using LightControl.ZWave;
using System;
using ZWave.CommandClasses;

namespace LightControl.Plugin.ZoozSensor
{
    public class ZoozMotionSensor : IMotionSensor
    {
        private readonly object _lock = new object();
        private readonly ZWaveController _controller;
        private readonly string _port;
        private readonly int _nodeId;
        private Basic _basicCommand;

        public ZoozMotionSensor(string port, int nodeId)
        {
            _port = port;
            _nodeId = nodeId;
            _controller = ZWaveControllerFactory.Instance.AcquireController(port);
            _controller.Connected += Controller_Connected;
        }

        public void Dispose()
        {
            lock (_lock)
                TryUnSubscribeBasicCommand(_basicCommand);
            ZWaveControllerFactory.Instance.ReleaseController(_port);
        }

        public event EventHandler MotionDetected;

        private async void Controller_Connected(object sender, EventArgs e)
        {
            var node = await _controller.GetNodeAsync(_nodeId);

            lock (_lock)
            {
                TryUnSubscribeBasicCommand(_basicCommand);

                if (node == null)
                    return;

                _basicCommand = node.GetCommandClass<Basic>();
                _basicCommand.Changed += BasicCommand_Changed;
                Logger.Log(this, LogLevel.Info, "Subscribed to new motion detection listener.");
            }
        }

        private void BasicCommand_Changed(object sender, ReportEventArgs<BasicReport> args)
        {
            if (args.Report.Value == 255)
                MotionDetected?.Invoke(this, EventArgs.Empty);
        }

        private void TryUnSubscribeBasicCommand(Basic basicCommand)
        {
            if (basicCommand == null)
                return;

            ActionUtils.TryDoAction(() => basicCommand.Changed -= BasicCommand_Changed, "Error unsubscribing from motion sensor.");
        }
    }
}

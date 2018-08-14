using HomeDeviceControl.Core;
using HomeDeviceControl.Core.Utils;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ZWaveLibrary = ZWave;

namespace HomeDeviceControl.ZWave
{
    public sealed class ZWaveController : IDisposable
    {
        private readonly object _lock = new object();
        private readonly string _port;
        private readonly EventHandlerList _events = new EventHandlerList();
        private ZWaveLibrary.ZWaveController _controller;
        private ZWaveLibrary.Channel.ISerialPort _serialPort;

        public ZWaveController(string port)
        {
            _port = port;
            TryInitialize();
        }

        public event EventHandler Connected
        {
            add
            {
                bool fireConnected = false;
                lock (_lock)
                {
                    _events.AddHandler(nameof(Connected), value);
                    fireConnected = _controller != null;
                }

                if (fireConnected)
                    value?.Invoke(this, EventArgs.Empty);
            }
            remove
            {
                lock (_lock)
                    _events.RemoveHandler(nameof(Connected), value);
            }
        }

        public void Dispose()
        {
            GetController()?.Close();
            (_serialPort as IDisposable)?.Dispose();
        }

        public async Task<ZWaveLibrary.Node> GetNodeAsync(int id)
        {
            var controller = GetController();
            if (controller == null)
                return null;

            try
            {
                var nodes = await controller.GetNodes();
                return nodes[(byte)id];
            }
            catch (Exception ex)
            {
                Logger.Log(this, LogLevel.Error, $"Problem getting node {id}", ex);
                return null;
            }
        }

        private void TryInitialize()
        {
            var controller = AttemptCreate();
            if (controller == null)
            {
                AttemptReconnection();
                return;
            }

            controller.Error += (sender, e) =>
            {
                Logger.Log(this, LogLevel.Error, $"Problem with zwave controller. Will attempt reconnection.", e.Error);
                RecreateController();
            };

            controller.ChannelClosed += (sender, e) =>
            {
                Logger.Log(this, LogLevel.Info, $"Channel closed with zwave controller.");
                AttemptReconnection();
            };

            lock (_lock)
                _controller = controller;

            FireConnectedEvent();
            LogNodes(controller);

            async void AttemptReconnection()
            {
                const int waitSeconds = 10;
                Logger.Log(this, LogLevel.Info, $"Waiting {waitSeconds}s before attempting zwave reconnection.");
                await Task.Delay(TimeSpan.FromSeconds(waitSeconds));
                // prevent stack from building up by running a new task
                var unwaitedTask = Task.Run(() => TryInitialize());
            }
        }

        private void RecreateController()
        {
            ZWaveLibrary.ZWaveController controller;
            lock (_lock)
            {
                controller = _controller;
                _controller = null;
            }

            if (controller != null)
                ActionUtils.TryDoAction(() => controller.Close(), "Problem closing the connection.");

            TryInitialize();
        }

        private ZWaveLibrary.ZWaveController AttemptCreate()
        {
            try
            {
                var controller = new ZWaveLibrary.ZWaveController(GetSerialPort());
                controller.Open();
                return controller;
            }
            catch (Exception ex)
            {
                Logger.Log(this, LogLevel.Error, "Problem creating zwave controller.", ex);
                return null;
            }
        }

        private async void LogNodes(ZWaveLibrary.ZWaveController controller)
        {
            var nodeIds = (await controller.GetNodes()).Select(n => n.NodeID);
            Logger.Log(this, LogLevel.Info, "ZWave node ids: " + string.Join(", ", nodeIds));
        }

        private ZWaveLibrary.Channel.ISerialPort GetSerialPort()
        {
            lock (_lock)
                _serialPort = _serialPort ?? new SerialPort(_port);
            return _serialPort;
        }

        private ZWaveLibrary.ZWaveController GetController()
        {
            lock (_lock)
                return _controller;
        }

        private void FireConnectedEvent()
        {
            EventHandler handler;
            lock (_lock)
                handler = (EventHandler)_events[nameof(Connected)];
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}

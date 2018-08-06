using System.Collections.Generic;
using ZWave;

namespace LightControl.ZWave
{
    public class ZWaveControllerFactory
    {
        private object _lock = new object();
        private Dictionary<string, int> _counts = new Dictionary<string, int>();
        private Dictionary<string, ZWaveController> _controllers = new Dictionary<string, ZWaveController>();

        public static ZWaveControllerFactory Instance { get; } = new ZWaveControllerFactory();

        private ZWaveControllerFactory()
        {
        }

        public ZWaveController AcquireController(string port)
        {
            lock (_lock)
            {
                if (!_controllers.ContainsKey(port))
                {
                    // todo: make this handle the case where the z wave controller is down
                    _controllers[port] = new ZWaveController(port);
                    _counts[port] = 0;
                }

                _counts[port]++;
                return _controllers[port];
            }
        }

        public void ReleaseController(string port)
        {
            lock (_lock)
            {
                if (!_counts.ContainsKey(port))
                    return;

                _counts[port]--;
                if (_counts[port] > 0)
                    return;

                _controllers[port].Dispose();
                _controllers.Remove(port);
                _counts.Remove(port);
            }
        }
    }
}

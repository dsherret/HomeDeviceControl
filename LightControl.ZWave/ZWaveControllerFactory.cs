using System;
using ZWave;

namespace LightControl.ZWave
{
    public class ZWaveControllerFactory
    {
        private object _lock = new object();
        private int _count = 0;
        private ZWaveController _controller;

        public static ZWaveControllerFactory Instance { get; } = new ZWaveControllerFactory();

        private ZWaveControllerFactory()
        {
        }

        public ZWaveController AcquireController()
        {
            lock (_lock)
            {
                if (_controller == null)
                {
                    // todo: make this handle the case where the z wave controller is down
                    _controller = new ZWaveController("COM4");
                    _controller.Open();
                }

                _count++;
                return _controller;
            }
        }

        public void ReleaseController()
        {
            lock (_lock)
            {
                _count--;
                if (_count > 0)
                    return;

                _controller.Close();
                _controller = null;
            }
        }
    }
}

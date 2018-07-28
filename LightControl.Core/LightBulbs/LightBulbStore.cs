using System;
using System.Collections.Generic;

namespace LightControl.Core.LightBulbs
{
    public class LightBulbStore
    {
        private readonly object _lock = new object();
        private readonly Dictionary<Guid, LightBulbContainer> _lightBulbs = new Dictionary<Guid, LightBulbContainer>();

        public ILightBulb Get(Guid id)
        {
            return GetLightBulbInternal(id);
        }

        internal void AddDiscoverer(ILightBulbDiscoverer discoverer)
        {
            discoverer.Discovered += Discoverer_Discovered;
        }

        private void Discoverer_Discovered(object sender, LightBulbDiscoveredEventArgs e)
        {
            Console.WriteLine($"Discovered: {e.LightBulb.Id}");
            GetLightBulbInternal(e.LightBulb.Id).SetLightBulb(e.LightBulb);
        }

        private LightBulbContainer GetLightBulbInternal(Guid id)
        {
            lock (_lock)
            {
                if (!_lightBulbs.ContainsKey(id))
                    return _lightBulbs[id] = new LightBulbContainer(id);
                return _lightBulbs[id];
            }
        }
    }
}

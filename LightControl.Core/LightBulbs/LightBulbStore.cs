using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LightControl.Core.LightBulbs
{
    public class LightBulbStore
    {
        private readonly object _lock = new object();
        private readonly Dictionary<Guid, LightBulbContainer> _lightBulbs = new Dictionary<Guid, LightBulbContainer>();
        private readonly EventHandlerList _eventHandlerList = new EventHandlerList();

        public event EventHandler<LightBulbEventArgs> Added
        {
            add
            {
                ILightBulb[] currentBulbs;

                lock (_lock)
                {
                    currentBulbs = GetAll();
                    _eventHandlerList.AddHandler(nameof(Added), value);
                }

                // retroactively fire event for all bulbs in the store for added handler
                foreach (var bulb in currentBulbs)
                    value(this, new LightBulbEventArgs(bulb));
            }
            remove
            {
                lock (_lock)
                    _eventHandlerList.RemoveHandler(nameof(Added), value);
            }
        }

        public ILightBulb Get(Guid id)
        {
            return GetLightBulbInternal(id);
        }

        public ILightBulb[] GetAll()
        {
            lock (_lock)
                return _lightBulbs.Values.ToArray();
        }

        internal void AddDiscoverer(ILightBulbDiscoverer discoverer)
        {
            discoverer.Discovered += Discoverer_Discovered;
        }

        private void Discoverer_Discovered(object sender, LightBulbEventArgs e)
        {
            Console.WriteLine($"Discovered: {e.LightBulb.Id}");
            GetLightBulbInternal(e.LightBulb.Id).SetLightBulb(e.LightBulb);
        }

        private LightBulbContainer GetLightBulbInternal(Guid id)
        {
            LightBulbContainer lightBulb;
            var fireAddedEvent = false;

            lock (_lock)
            {
                if (_lightBulbs.ContainsKey(id))
                    lightBulb = _lightBulbs[id];
                else
                {
                    lightBulb = _lightBulbs[id] = new LightBulbContainer(id);
                    fireAddedEvent = true;
                }
            }

            if (fireAddedEvent)
                FireAddedEvent(lightBulb);

            return lightBulb;
        }

        private void FireAddedEvent(ILightBulb bulb)
        {
            ((EventHandler<LightBulbEventArgs>)_eventHandlerList[nameof(Added)])?.Invoke(this, new LightBulbEventArgs(bulb));
        }
    }
}

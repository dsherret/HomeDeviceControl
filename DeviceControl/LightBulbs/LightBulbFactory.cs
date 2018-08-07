using DeviceControl.Core.LightBulbs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DeviceControl.LightBulbs
{
    public sealed class LightBulbFactory : IDisposable
    {
        private readonly object _lock = new object();
        private readonly Dictionary<ILightBulb, LightBulbWrapper> _lightBulbs = new Dictionary<ILightBulb, LightBulbWrapper>();
        private readonly EventHandlerList _eventHandlerList = new EventHandlerList();
        private readonly LightBulbStore _store;

        public LightBulbFactory(LightBulbStore store)
        {
            _store = store;
            store.Added += Store_Added;
        }

        public void Dispose()
        {
            _store.Added -= Store_Added;
        }

        public event EventHandler<LightBulbWrapperEventArgs> Added
        {
            add
            {
                LightBulbWrapper[] currentBulbs;

                lock (_lock)
                {
                    currentBulbs = _store.GetAll().Select(l => GetWrapper(l)).ToArray();
                    _eventHandlerList.AddHandler(nameof(Added), value);
                }

                // retroactively fire event for all bulbs in the store for added handler
                foreach (var bulb in currentBulbs)
                    value(this, new LightBulbWrapperEventArgs(bulb));
            }
            remove
            {
                _eventHandlerList.RemoveHandler(nameof(Added), value);
            }
        }

        public LightBulbWrapper Get(Guid id)
        {
            return GetWrapper(_store.Get(id));
        }

        private void Store_Added(object sender, LightBulbEventArgs e)
        {
            // add it to the internal cache
            var wrapper = GetWrapper(e.Value);

            // fire event
            EventHandler<LightBulbWrapperEventArgs> handler;
            lock (_lock)
                handler = (EventHandler<LightBulbWrapperEventArgs>)_eventHandlerList[nameof(Added)];
            handler?.Invoke(this, new LightBulbWrapperEventArgs(wrapper));
        }

        private LightBulbWrapper GetWrapper(ILightBulb bulb)
        {
            lock (_lock)
            {
                if (_lightBulbs.ContainsKey(bulb))
                    return _lightBulbs[bulb];
                var wrapper = new LightBulbWrapper(bulb);
                _lightBulbs.Add(bulb, wrapper);
                return wrapper;
            }
        }
    }
}

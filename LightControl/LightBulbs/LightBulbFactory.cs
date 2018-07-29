using LightControl.Core.LightBulbs;
using System;
using System.Collections.Generic;

namespace LightControl.LightBulbs
{
    public class LightBulbFactory
    {
        private readonly object _lock = new object();
        private readonly Dictionary<ILightBulb, LightBulbWrapper> _lightBulbs = new Dictionary<ILightBulb, LightBulbWrapper>();
        private readonly LightBulbStore _store;

        public LightBulbFactory(LightBulbStore store)
        {
            store.Added += (sender, e) =>
            {
                // add it to the internal cache
                GetWrapper(e.LightBulb);
            };
            _store = store;
        }

        public LightBulbWrapper Get(Guid id)
        {
            return GetWrapper(_store.Get(id));
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

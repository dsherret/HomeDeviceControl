﻿using LightControl.Core.LightBulbs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YeelightAPI;

namespace LightControl.Plugin.Yeelight
{
    public class YeelightBulbDiscoverer : ILightBulbDiscoverer
    {
        public async Task<IEnumerable<ILightBulb>> DiscoverAsync()
        {
            var devices = await DeviceLocator.Discover();
            return devices.Select(d => new YeelightBulb(d))
                .ToArray(); // collection could be modified so cache its current state
        }
    }
}

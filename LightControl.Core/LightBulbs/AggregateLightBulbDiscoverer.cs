using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightControl.Core.LightBulbs
{
    internal class AggregateLightBulbDiscoverer : ILightBulbDiscoverer
    {
        private readonly ILightBulbDiscoverer[] _lightBulbDiscoverers;

        public AggregateLightBulbDiscoverer(ILightBulbDiscoverer[] lightBulbDiscoverers)
        {
            _lightBulbDiscoverers = lightBulbDiscoverers;
        }

        public async Task<IEnumerable<ILightBulb>> Discover()
        {
            var lightBulbCollections = await Task.WhenAll(_lightBulbDiscoverers.Select(d => d.Discover()));
            return lightBulbCollections.SelectMany(b => b);
        }
    }
}

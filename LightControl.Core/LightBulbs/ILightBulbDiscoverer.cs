using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightControl.Core.LightBulbs
{
    public interface ILightBulbDiscoverer
    {
        Task<IEnumerable<ILightBulb>> DiscoverAsync();
    }
}

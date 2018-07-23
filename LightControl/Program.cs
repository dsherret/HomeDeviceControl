using LightControl.Core;
using System;
using System.Threading.Tasks;

namespace LightControl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pluginSystem = new PluginSystem();
            var lightBulbs = await pluginSystem.GetLightBulbDiscoverer().Discover();
            foreach (var lightBulb in lightBulbs)
            {
                await lightBulb.Connect();
                await lightBulb.SetRGBColor(200, 100, 0);
            }
            Console.ReadKey();
        }
    }
}

using HomeDeviceControl.Core.LightBulbs;
using HomeDeviceControl.Core.Sensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HomeDeviceControl.Core
{
    public class PluginSystem
    {
        private readonly Assembly[] _assemblies;

        public PluginSystem()
        {
            _assemblies = GetPluginAssemblies().ToArray();
            if (!_assemblies.Any())
                throw new Exception("Could not find any plugin assemblies.");

            foreach (var discoverer in GetLightBulbDiscoverers())
                LightBulbStore.AddDiscoverer(discoverer);
        }

        public LightBulbStore LightBulbStore { get; } = new LightBulbStore();

        private IEnumerable<ILightBulbDiscoverer> GetLightBulbDiscoverers()
        {
            var discovererType = typeof(ILightBulbDiscoverer);
            var discoverers = GetDiscoverers().ToArray();

            if (!discoverers.Any())
                throw new Exception("Could not find any plugins containing a discoverer.");

            return discoverers;

            IEnumerable<ILightBulbDiscoverer> GetDiscoverers()
            {
                return _assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.GetInterface(discovererType.FullName) != null)
                    .Select(t => Activator.CreateInstance(t) as ILightBulbDiscoverer)
                    .Where(d => d != null);
            }
        }

        private static IEnumerable<Assembly> GetPluginAssemblies()
        {
            var isPluginFile = new Regex(@".*\.Plugin\..*\.dll$", RegexOptions.IgnoreCase);

            return Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory)
                .Where(d => isPluginFile.IsMatch(Path.GetFileName(d)))
                .Select(file => Assembly.Load(AssemblyName.GetAssemblyName(file)));
        }
    }
}

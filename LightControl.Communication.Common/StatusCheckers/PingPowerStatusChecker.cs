using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace LightControl.Communication.Common.StatusCheckers
{
    /// <summary>
    /// Pings a device for its status.
    /// </summary>
    public class PingPowerStatusChecker : IPowerStatusChecker
    {
        private readonly string _hostnameOrIpAddr;

        public PingPowerStatusChecker(string hostnameOrIpAddr)
        {
            _hostnameOrIpAddr = hostnameOrIpAddr;
        }

        public async Task<bool> GetIsPoweredOnAsync()
        {
            using (var ping = new Ping())
            {
                var result = await ping.SendPingAsync(_hostnameOrIpAddr);
                return result.Status == IPStatus.Success;
            }
        }
    }
}

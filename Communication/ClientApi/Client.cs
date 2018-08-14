using HomeDeviceControl.Communication.Common;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HomeDeviceControl.Communication.ClientApi
{
    /// <summary>
    /// Api for communicating with the server.
    /// </summary>
    public sealed class Client : IDisposable
    {
        private readonly string _serverUrl;
        private readonly HttpClient _client;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serverUrl">The address to communicate with the server at (Ex. http://192.168.1.125:8084).</param>
        public Client(string serverUrl)
        {
            _serverUrl = serverUrl;
            _client = new HttpClient();
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }

        /// <summary>
        /// Tell the server if the device is powered on.
        /// </summary>
        /// <param name="deviceId">Device identifier.</param>
        /// <param name="isPoweredOn">If the device is powered on.</param>
        public Task UpdateDevicePowerStatus(Guid deviceId, bool isPoweredOn)
        {
            return ClientPostValueAsync(Routes.DevicePowerStatus, new DevicePowerStatus
            {
                DeviceId = deviceId,
                IsPoweredOn = isPoweredOn
            });
        }

        private Task ClientPostValueAsync<T>(string route, T value)
        {
            var content = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
            return ClientPostAsync(route, content);
        }

        private async Task ClientPostAsync(string route, HttpContent content)
        {
            var url = _serverUrl + route;
            var response = await _client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error connecting to server: {response.StatusCode}");
        }
    }
}

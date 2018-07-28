using System;
using System.Net.Http;

namespace LightControl.Communication.ClientApi
{
    public sealed class Client
    {
        private readonly string _url;
        private readonly HttpClient _client;

        public Client(string url)
        {
            _url = url;
            _client = new HttpClient();
        }


    }
}

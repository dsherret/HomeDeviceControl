using System;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;

namespace LightControl.Communication.Server
{
    public sealed class Server : IDisposable
    {
        private readonly WebServer _server;

        public Server(int port)
        {
            _server = new WebServer(port);
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        public void AddValueListener<T>(IValueListener<T> listener) where T : class
        {
            var module = new WebApiModule();
            module.AddHandler(listener.UrlRoute, Unosquare.Labs.EmbedIO.Constants.HttpVerbs.Post, (context, token) => {
                var value = context.ParseJson<T>();
                listener.OnValueReceived(value);
                return context.JsonResponseAsync(true);
            });
            _server.RegisterModule(module);
        }

        public Task RunAsync(CancellationToken token = default)
        {
            return _server.RunAsync();
        }
    }
}

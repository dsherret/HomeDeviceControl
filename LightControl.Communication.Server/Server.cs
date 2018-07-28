using LightControl.Communication.Common;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Labs.EmbedIO.Modules;

namespace LightControl.Communication.Server
{
    public sealed class Server : IDisposable
    {
        private readonly WebServer _server;

        /// <summary>
        /// Need this in order to define handlers at runtime.
        /// </summary>
        private class RuntimeHandlerWebModule : WebModuleBase
        {
            public override string Name => nameof(RuntimeHandlerWebModule);
        }

        public Server(int port)
        {
            _server = new WebServer($"http://localhost:{port}/");
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        public void AddValueListener<T>(IValueListener<T> listener)
        {
            Console.WriteLine($"Adding listener at route: {listener.UrlRoute}");
            GetModule().AddHandler(listener.UrlRoute, HttpVerbs.Post, (context, token) => {
                var tType = typeof(T);
                if (tType == typeof(bool) || tType == typeof(string) || tType == typeof(int))
                {
                    var valueObj = JsonConvert.DeserializeObject<ValueObject<T>>(context.RequestBody());
                    listener.OnValueReceived(valueObj.Value);
                }
                else
                {
                    var obj = JsonConvert.DeserializeObject<T>(context.RequestBody());
                    listener.OnValueReceived(obj);
                }

                return context.JsonResponseAsync(new ValueObject<bool>(true));
            });
        }

        public Task RunAsync(CancellationToken token = default)
        {
            return _server.RunAsync();
        }

        private WebModuleBase GetModule()
        {
            var module = _server.Module<RuntimeHandlerWebModule>();
            if (module != null)
                return module;

            module = new RuntimeHandlerWebModule();
            _server.RegisterModule(module);
            return module;
        }
    }
}

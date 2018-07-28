using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace LightControl.Core.LightBulbs
{
    internal class LightBulbContainer : ILightBulb
    {
        private class State
        {
            public bool IsPoweredOn { get; set; }
            public Color Color { get; set; }
        }

        private readonly State _state = new State();
        private readonly HashSet<string> _pendingState = new HashSet<string>();
        private ILightBulb _lightBulb;

        public LightBulbContainer(Guid id)
        {
            Id = id;
        }

        internal void SetLightBulb(ILightBulb lightBulb)
        {
            _lightBulb?.Dispose();
            _lightBulb = lightBulb;
            var unusedTask = ConnectAsync();
        }

        public Guid Id { get; }
        public bool IsConnected => _lightBulb?.IsConnected ?? false;

        public async Task ConnectAsync()
        {
            await (_lightBulb?.ConnectAsync() ?? Task.CompletedTask);
            await SyncState();
        }

        public void Dispose()
        {
            _lightBulb?.Dispose();
        }

        public Task TurnOnAsync()
        {
            _state.IsPoweredOn = true;
            _pendingState.Add(nameof(_state.IsPoweredOn));
            return SyncState();
        }

        public Task TurnOffAsync()
        {
            _state.IsPoweredOn = false;
            _pendingState.Add(nameof(_state.IsPoweredOn));
            return SyncState();
        }

        public Task SetRGBColorAsync(Color color)
        {
            _state.Color = color;
            _pendingState.Add(nameof(_state.Color));
            return SyncState();
        }

        private Task SyncState()
        {
            if (!IsConnected)
                return Task.CompletedTask;

            var tasks = new List<Task>();

            if (_pendingState.Remove(nameof(_state.IsPoweredOn)))
                tasks.Add(_state.IsPoweredOn ? _lightBulb.TurnOnAsync() : _lightBulb.TurnOffAsync());
            if (_pendingState.Remove(nameof(_state.Color)))
                tasks.Add(_lightBulb.SetRGBColorAsync(_state.Color));

            return Task.WhenAll(tasks);
        }
    }
}

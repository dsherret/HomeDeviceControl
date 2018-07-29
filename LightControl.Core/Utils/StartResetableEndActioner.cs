using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightControl.Core.Utils
{
    /// <summary>
    /// Executes a start action, delays for a specified amount of time, then executes the end action if the method hasn't been called before.
    /// </summary>
    public class StartResetableEndActioner
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly Func<Task> _startAction;
        private readonly Func<Task> _endAction;
        private readonly TimeSpan _delay;
        private CancellationTokenSource _cts;

        public StartResetableEndActioner(Func<Task> startAction, Func<Task> endAction, TimeSpan delay)
        {
            _startAction = startAction;
            _endAction = endAction;
            _delay = delay;
        }

        /// <summary>
        /// Executes the start action and resets the end action to happen after a delay.
        /// </summary>
        public async Task DoActions()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                }

                await _startAction();

                _cts = new CancellationTokenSource();
                var token = _cts.Token;
                var unusedTask = Task.Run(() => TryDoEndAction(token), token);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task TryDoEndAction(CancellationToken token)
        {
            await Task.Delay(_delay, token);
            await _semaphore.WaitAsync();

            try
            {
                token.ThrowIfCancellationRequested();

                await _endAction();
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _semaphore.Release();
            }
        }
    }
}

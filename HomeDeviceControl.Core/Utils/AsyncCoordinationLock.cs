using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeDeviceControl.Core.Utils
{
    public class AsyncCoordinationLock
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public Task DoActionAsync(Action action)
        {
            return DoActionAsync(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }

                return Task.CompletedTask;
            });
        }

        public async Task DoActionAsync(Func<Task> action)
        {
            await _semaphore.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}

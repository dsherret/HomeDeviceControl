using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceControl.Core.Utils
{
    public class AsyncEventContainer<T>
    {
        private readonly List<Func<T, Task>> _actions = new List<Func<T, Task>>();

        public void Add(Func<T, Task> action)
        {
            _actions.Add(action);
        }

        public async Task InvokeAsync(T obj)
        {
            foreach (var action in _actions)
                await action(obj);
        }
    }
}

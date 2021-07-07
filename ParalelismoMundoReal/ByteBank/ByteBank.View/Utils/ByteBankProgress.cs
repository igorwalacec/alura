using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ByteBank.View.Utils
{
    public class ByteBankProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;
        private readonly TaskScheduler _taskScheduler;

        public ByteBankProgress(Action<T> handler, TaskScheduler taskScheduler)
        {
            _handler = handler;
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        public void Report(T value)
        {
            Task.Factory.StartNew(
                () => _handler(value),
                CancellationToken.None,
                TaskCreationOptions.None,
                _taskScheduler
                );
        }
    }
}

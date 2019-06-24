using System;
using System.Threading;

namespace IoTSharp.Diagnostics
{
    public class OperationsPerSecondCounter
    {
        private int _current;

        public OperationsPerSecondCounter(string uid)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        }

        public string Uid { get; }

        public int Count { get; private set; }

        public void Increment()
        {
            Interlocked.Increment(ref _current);
        }

        public void Reset()
        {
            Count = Interlocked.Exchange(ref _current, 0);
        }
    }
}

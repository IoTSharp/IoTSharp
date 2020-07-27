using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Queue
{
    public interface IMsgQueue
    {
        public void Enqueue(RawMsg msg);
        public RawMsg Dequeue();
    }
}

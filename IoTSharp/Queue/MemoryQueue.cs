using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IoTSharp.Queue
{
    public class MemoryQueue : IMsgQueue
    {
        Queue<RawMsg> _raws;
        public MemoryQueue()
        {
            _raws = new Queue<RawMsg>();
        }
        public void Enqueue(RawMsg msg)
        {
            _raws.Enqueue(msg);
        }
        public RawMsg Dequeue()
        {
            return _raws.TryDequeue(out RawMsg raw) ? raw : null;
        }

    }
}

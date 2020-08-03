using Org.BouncyCastle.Asn1;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IoTSharp.Queue
{
    public class SimpleQueue : IMsgQueue
    {
        Priority_Queue.SimplePriorityQueue<RawMsg> _raws;
        public SimpleQueue()
        {
            _raws = new Priority_Queue.SimplePriorityQueue<RawMsg>();
        }
        public void Enqueue(RawMsg msg)
        {
            _raws.Enqueue(msg,0);
        }
        public RawMsg Dequeue()
        {
            return _raws.Dequeue();
        }

        public void Flush()
        {
        }
    }
}

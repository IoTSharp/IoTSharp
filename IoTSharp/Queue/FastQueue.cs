using Org.BouncyCastle.Asn1;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IoTSharp.Queue
{
    public class FastQueue : IMsgQueue
    {
        public class Node : FastPriorityQueueNode
        {
            public RawMsg Msg { get; private set; }
            public Node(RawMsg name)
            {
                Msg = name;
            }
        }

        private const int MAX_USERS_IN_QUEUE = 1000000;
        Priority_Queue.FastPriorityQueue<Node> _raws;
        public FastQueue()
        {


            _raws = new Priority_Queue.FastPriorityQueue<Node>(MAX_USERS_IN_QUEUE);
        }
        public void Enqueue(RawMsg msg)
        {
            lock (this)
            {

                _raws.Enqueue(new Node(msg), 0);
            }
        }
        public RawMsg Dequeue()
        {
            lock (this)
            {
                return _raws.Dequeue()?.Msg;
            }

        }

        public void Flush()
        {
        }
    }
}

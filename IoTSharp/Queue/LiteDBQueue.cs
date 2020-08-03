using LiteDB;
using LiteQueue;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Queue
{
    public    class LiteDBQueue : LiteQueue<RawMsg>,IMsgQueue
    {
        public LiteDBQueue() : base(new LiteDatabase("DiskQueue.db"))
        {

        }

        public LiteDBQueue(string ConnectionString):base(new LiteDatabase(ConnectionString))
        {
       
        }
        public new void Enqueue(RawMsg msg)
        {
          base.Enqueue(msg);
        }
        public new RawMsg Dequeue()
        {
            RawMsg result = null;
            if (this.Count() > 0)
            {
                var msg = base.Dequeue();
                if (msg != null)
                {
                    result = msg.Payload;
                    try
                    {
                        base.Commit(msg);
                    }
                    catch (Exception)
                    {
                        base.Abort(msg);
                        result = null;
                    }
                }
            }
            return result;
        }

        public void Flush()
        {
        }
    }
}

using LiteDB;
using LiteQueue;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Queue
{
    public    class DiskQueue : LiteQueue<RawMsg>,IMsgQueue
    {
        public DiskQueue(string ConnectionString):base(new LiteDatabase(ConnectionString))
        {
       
        }
        public new void Enqueue(RawMsg msg)
        {
          base.Enqueue(msg);
        }
        public new RawMsg Dequeue()
        {
            var msg = base.Dequeue();
            var result = msg.Payload;
            try
            {
                base.Commit(msg);
            }
            catch (Exception)
            {
                base.Abort(msg);
                result = null;
            }
            return result;
        }

    }
}

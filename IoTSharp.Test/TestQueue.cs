using IoTSharp.Queue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Test
{
    [TestClass]
    public class TestQueue
    {
        Dictionary<string, object> dic=new Dictionary<string, object> ();
        [TestInitialize]
        public void InitTest()
        {
            for (int ix = 0; ix < 10; ix++)
            {
                dic.Add($"key_{ix}", GenerateCheckCodeNum(1000));
            }
        }

      
        [TestMethod]
        public void TestSimpleQueue() => TestIMsgQueue<SimpleQueue>();

        [TestMethod]
        public void TestFastQueue() => TestIMsgQueue<FastQueue>();

        [TestMethod]
        public void TestDiskQueue() => TestIMsgQueue<LiteDBQueue>();



        [TestMethod]
        public void TestMemoryQueue() => TestIMsgQueue<MemoryQueue>();

        public void TestIMsgQueue<T>() where T : IMsgQueue, new()
        {
            List<Task> tasks = new List<Task>();
            var t = new T();
            for (int i = 0; i < 10000; i++)
            {
                tasks.Add( Task.Run(()=>   t.Enqueue(new RawMsg() { DataCatalog = Data.DataCatalog.TelemetryData, DataSide = Data.DataSide.AnySide, DeviceId = Guid.NewGuid(), MsgBody = dic, MsgType = MsgType.MQTT })));
            }
            for (int i = 0; i < 10000; i++)
            {
                tasks.Add(Task.Run(() => t.Dequeue()));
            }
            Task.WaitAll(tasks.ToArray());
        }
        private int rep = 0;
        private string GenerateCheckCodeNum(int codeCount)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + this.rep;
            this.rep++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> this.rep)));
            for (int i = 0; i < codeCount; i++)
            {
                int num = random.Next();
                str = str + ((char)(0x30 + ((ushort)(num % 10)))).ToString();
            }
            return str;
        }
    }
}

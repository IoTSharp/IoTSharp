using IoTSharp.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace IoTSharp.Test
{
    [TestClass]
    public class JsonConverTest
    {
        [TestMethod]
        public void TestJsonObject()
        {
            var jojb = JToken.Parse("{\"aaa\":\"bbb\"}");
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            jojb.Children().ToList().ForEach(a => keyValues.Add(((JProperty)a).Name, ((JProperty)a).JPropertyToObject()));
            Assert.AreEqual<string>("bbb", keyValues["aaa"].ToString());
        }

        [TestMethod]
        public void TestJsonArray()
        {
            var jojb = JToken.Parse("[{\"aaa\":\"bbb\"},{\"ccc\":\"ddd\"}]");
            Dictionary<string, object> keyValues = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jojb.ToString());
            Assert.AreEqual<string>("bbb", keyValues["aaa"].ToString());
            Assert.AreEqual<string>("ddd", keyValues["ccc"].ToString());
        }

        [TestMethod]
        public void TestDic()
        {
            var sss = new Dictionary<string, object>();
            sss.Add("eee", "fff");
            sss.Add("ggg", "hhh");
            sss.Add("iii", "kkk");
            Assert.IsNotNull(Newtonsoft.Json.JsonConvert.SerializeObject(sss));
        }
    }
}
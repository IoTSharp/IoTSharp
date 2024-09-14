using IoTSharp.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using IoTSharp.Data;
using IoTSharp.Contracts;
using IoTSharp.Dtos;
using System.Text.Json;
using System;

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
        public void TestDic()
        {
            var sss = new Dictionary<string, object>();
            sss.Add("eee", "fff");
            sss.Add("ggg", "hhh");
            sss.Add("iii", "kkk");
            Assert.IsNotNull(Newtonsoft.Json.JsonConvert.SerializeObject(sss));
        }
        [TestMethod]
        public void JsonSerializer_ApiResult_InstanceDto()
        {
            var js = new ApiResult<InstanceDto>(ApiCode.Success, "OK", new InstanceDto() { Installed = true , Version=DateTime.Now.ToString()});
            var json=  JsonSerializer.Serialize(js);
            var result= JsonSerializer.Deserialize <ApiResult<InstanceDto>>(json,new JsonSerializerOptions() { IncludeFields=true });
            Assert.AreEqual(js.Data.Installed, result.Data.Installed);
            Assert.AreEqual(js.Data.Version, result.Data.Version);
        }
    }
}
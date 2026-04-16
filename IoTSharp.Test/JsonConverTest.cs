using IoTSharp.Extensions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using IoTSharp.Data;
using IoTSharp.Contracts;
using IoTSharp.Dtos;
using System.Text.Json;
using System;
using Xunit;

namespace IoTSharp.Test
{
    public class JsonConverTest
    {
        [Fact]
        public void TestJsonObject()
        {
            var jojb = JToken.Parse("{\"aaa\":\"bbb\"}");
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            jojb.Children().ToList().ForEach(a => keyValues.Add(((JProperty)a).Name, ((JProperty)a).JPropertyToObject()));
            Assert.Equal("bbb", keyValues["aaa"].ToString());
        }



        [Fact]
        public void TestDic()
        {
            var sss = new Dictionary<string, object>();
            sss.Add("eee", "fff");
            sss.Add("ggg", "hhh");
            sss.Add("iii", "kkk");
            Assert.NotNull(Newtonsoft.Json.JsonConvert.SerializeObject(sss));
        }

        [Fact]
        public void JsonSerializer_ApiResult_InstanceDto()
        {
            var js = new ApiResult<InstanceDto>(ApiCode.Success, "OK", new InstanceDto() { Installed = true, Version = DateTime.Now.ToString() });
            var json = JsonSerializer.Serialize(js);
            var result = JsonSerializer.Deserialize<ApiResult<InstanceDto>>(json, new JsonSerializerOptions() { IncludeFields = true });
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(js.Data.Installed, result.Data.Installed);
            Assert.Equal(js.Data.Version, result.Data.Version);
        }
    }
}
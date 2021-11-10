using IoTSharp.Interpreter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Test
{
    [TestClass]
    public class ScriptEngineTest
    {
        private JavaScriptEngine _js_engine;
        private PythonScriptEngine _python_engine;
        private LuaScriptEngine _lua_engine;
        private CScriptEngine _c_engine;
        private SQLEngine _sql_engine;
        private CSharpScriptEngine _csharp_engine;

        [TestInitialize]
        public void InitTestScriptEngine()
        {
            var lgf = LoggerFactory.Create(f =>
              {
                  f.AddConsole();
              });
           
          
             _js_engine = new JavaScriptEngine(lgf.CreateLogger<JavaScriptEngine>(), Options.Create( new Interpreter.EngineSetting() { Timeout = 4 }));
            _python_engine = new PythonScriptEngine(lgf.CreateLogger<PythonScriptEngine>(), Options.Create(new Interpreter.EngineSetting() { Timeout = 4 }));
            _lua_engine = new  LuaScriptEngine (lgf.CreateLogger<LuaScriptEngine>(), Options.Create(new Interpreter.EngineSetting() { Timeout = 4 }));
            _c_engine = new CScriptEngine(lgf.CreateLogger<CScriptEngine>(), Options.Create(new Interpreter.EngineSetting() { Timeout = 4 }));
            _sql_engine=new SQLEngine(lgf.CreateLogger<SQLEngine>(), Options.Create(new Interpreter.EngineSetting() { Timeout = 4 }));
            _csharp_engine = new  CSharpScriptEngine   (lgf.CreateLogger<CSharpScriptEngine>(), Options.Create(new Interpreter.EngineSetting() { Timeout = 4 }));
        }
        [TestMethod]
        public void TestJavaScript()
        {
            var intput = System.Text.Json.JsonSerializer.Serialize(new { temperature = 39, height = 192, weight = 121 });

            string output = _js_engine.Do(@"
var _m = (input.height / 100);
var output = {
    fever: input.temperature > 38 ? true : false,
    fat: input.weight / (_m * _m)>28?true:false
};
return output;
", intput);

            var t = new { fever = true, fat = true };
            var outpuobj = System.Text.Json.JsonSerializer.Deserialize(output, t.GetType());
            Assert.AreEqual(outpuobj, t);
        }

        [TestMethod]
        public void TestPython()
        {
            var intput = System.Text.Json.JsonSerializer.Serialize(new { temperature = 39, height = 192, weight = 121 });

            string output = _python_engine.Do(@"
_m=input.height/100
fever=True if input.temperature > 38 else False
fat=True if  input.weight / (_m * _m)>28 else False
output= {'fever': fever, 'fat': fat}
", intput);
            var t = new { fever = true, fat = true };
            var outpuobj = System.Text.Json.JsonSerializer.Deserialize(output, t.GetType());
            Assert.AreEqual(outpuobj, t);
        }
        [TestMethod]
        public void TestLua()
        {
            var intput = System.Text.Json.JsonSerializer.Serialize(new { temperature = 39, height = 192, weight = 121 });

            string output = _lua_engine.Do(@"
fff = {fever = false, fat = false}

function fff:new()
 local  o =  {}
  setmetatable(o, self)
  self.__index = self
  self.fever = true
  self.fat =  true
  return o
end

return    fff:new()
", intput);
            var t = new { fever = true, fat = true };
            var outpuobj = System.Text.Json.JsonSerializer.Deserialize(output, t.GetType());
            Assert.AreEqual(outpuobj, t);
        }
        [TestMethod]
        public void TestLua1()
        {
            var intput = System.Text.Json.JsonSerializer.Serialize(new { temperature = 39, height = 192, weight = 121 });
            string output = _lua_engine.Do( Properties.Resources.luatest, intput);
            var t = new { fever = true, fat = true };
            var outpuobj = System.Text.Json.JsonSerializer.Deserialize(output, t.GetType());
            Assert.AreEqual(outpuobj, t);
        }
        [TestMethod]
        public void TestC()
        {
            var input = "38";
            string output = _c_engine.Do("atoi(input)>38?1:0;", input);
            
            Assert.AreEqual(int.Parse(input) > 38 ? 1 : 0, output);
        }
        [TestMethod]
        public void TestSQL()
        {
            var sql = "select sex from input where (username=\"李红\")";
            var input = "[{\"username\":\"张三\",\"sex\":\"男\",\"birthday\":{\"year\":2000,\"month\":6,\"day\":18}},{\"username\":\"李红\",\"sex\":\"女\",\"birthday\":{\"year\":1986,\"month\":9,\"day\":22}}]";
            string output = _sql_engine.Do(sql,input );
            dynamic obj = JsonConvert.DeserializeObject<List<ExpandoObject>>(output);
            Assert.AreEqual(obj[0].sex, "女");
        }

        [TestMethod]
        public void TestCSharpScript()
        {
            var intput = System.Text.Json.JsonSerializer.Serialize(new { temperature = 39, height = 192, weight = 121 });

            string output = _csharp_engine.Do(@"
var _m = (input.height / 100);
var output = new {
    fever= input.temperature > 38 ? true : false,
    fat= input.weight / (_m * _m)>28?true:false
};
return output;
", intput);

            var t = new { fever = true, fat = true };
            var outpuobj = System.Text.Json.JsonSerializer.Deserialize(output, t.GetType());
            Assert.AreEqual(outpuobj, t);
        }
    }
}

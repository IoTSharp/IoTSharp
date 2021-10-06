using IoTSharp.Interpreter;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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

        [TestInitialize]
        public void InitTestScriptEngine()
        {
            var lgf = LoggerFactory.Create(f =>
              {
                  f.AddConsole();
              });
           
            _js_engine = new JavaScriptEngine(lgf.CreateLogger<JavaScriptEngine>(), new Interpreter.EngineSetting() { Timeout = 4 }, System.Threading.Tasks.Task.Factory.CancellationToken);
            _python_engine = new PythonScriptEngine(lgf.CreateLogger<PythonScriptEngine>(), new Interpreter.EngineSetting() { Timeout = 4 }, System.Threading.Tasks.Task.Factory.CancellationToken);
            _lua_engine = new  LuaScriptEngine (lgf.CreateLogger<LuaScriptEngine>(), new Interpreter.EngineSetting() { Timeout = 4 }, System.Threading.Tasks.Task.Factory.CancellationToken);

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
    }
}

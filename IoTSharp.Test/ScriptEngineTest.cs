using IoTSharp.Interpreter;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace IoTSharp.Test
{
    [TestClass]
    public class ScriptEngineTest
    {
        [TestMethod]
        public void JavaScriptEngine_CanTransformInput()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new JavaScriptEngine(loggerFactory.CreateLogger<JavaScriptEngine>(), CreateOptions());

            var input = JsonSerializer.Serialize(new { temperature = 39, height = 192, weight = 121 });
            var output = engine.Do(
                """
                var m = input.height / 100;
                return {
                    fever: input.temperature > 38,
                    fat: input.weight / (m * m) > 28
                };
                """,
                input);

            AssertHealthOutput(output);
        }

        [TestMethod]
        public void PythonEngine_CanTransformInput()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new PythonScriptEngine(loggerFactory.CreateLogger<PythonScriptEngine>(), CreateOptions());

            var input = JsonSerializer.Serialize(new { temperature = 39, height = 192, weight = 121 });
            var output = engine.Do(
                """
                m = input.height / 100
                fever = True if input.temperature > 38 else False
                fat = True if input.weight / (m * m) > 28 else False
                output = {'fever': fever, 'fat': fat}
                """,
                input);

            AssertHealthOutput(output);
        }

        [TestMethod]
        public void LuaEngine_CanTransformInput()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new LuaScriptEngine(loggerFactory.CreateLogger<LuaScriptEngine>(), CreateOptions());

            var input = JsonSerializer.Serialize(new { temperature = 39, height = 192, weight = 121 });
            var output = engine.Do(
                """
                local m = input.height / 100
                return {
                    fever = input.temperature > 38,
                    fat = input.weight / (m * m) > 28
                }
                """,
                input);

            AssertHealthOutput(output);
        }

        [TestMethod]
        public void CEngine_CanEvaluateExpression()
        {
            using var loggerFactory = CreateLoggerFactory();
            var engine = new CScriptEngine(loggerFactory.CreateLogger<CScriptEngine>(), CreateOptions());

            var output = engine.Do("2 + 3", "{}");
            Assert.AreEqual(5, JsonSerializer.Deserialize<int>(output));
        }

        [TestMethod]
        public void SqlEngine_CanQueryJsonPayload()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new SQLEngine(loggerFactory.CreateLogger<SQLEngine>(), CreateOptions());

            var sql = "select sex from input where (username=\"alice\")";
            var input = "[{\"username\":\"bob\",\"sex\":\"male\"},{\"username\":\"alice\",\"sex\":\"female\"}]";
            var output = engine.Do(sql, input);

            using var document = JsonDocument.Parse(output);
            Assert.AreEqual("female", document.RootElement[0].GetProperty("sex").GetString());
        }

        [TestMethod]
        public void CSharpEngine_CanReuseCompiledScript()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var cache = new MemoryCache(new MemoryCacheOptions());
            using var engine = new CSharpScriptEngine(loggerFactory.CreateLogger<CSharpScriptEngine>(), CreateOptions(), cache);

            var input = JsonSerializer.Serialize(new { temperature = 39, height = 192, weight = 121 });
            const string script =
                """
                var m = input.height / 100;
                var output = new
                {
                    fever = input.temperature > 38,
                    fat = input.weight / (m * m) > 28
                };
                return output;
                """;

            for (var i = 0; i < 10; i++)
            {
                var output = engine.Do(script, input);
                AssertHealthOutput(output);
            }
        }

        [TestMethod]
        public void BasicEngine_ReturnsInputPayload()
        {
            using var loggerFactory = CreateLoggerFactory();
            var engine = new BASICScriptEngine(loggerFactory.CreateLogger<BASICScriptEngine>(), CreateOptions());

            var input = JsonSerializer.Serialize(new { device = "device-001", enabled = true });
            var output = engine.Do("ignored", input);

            using var document = JsonDocument.Parse(output);
            Assert.AreEqual("device-001", document.RootElement.GetProperty("device").GetString());
            Assert.IsTrue(document.RootElement.GetProperty("enabled").GetBoolean());
        }

        private static IOptions<EngineSetting> CreateOptions()
            => Options.Create(new EngineSetting { Timeout = 4 });

        private static ILoggerFactory CreateLoggerFactory()
            => LoggerFactory.Create(_ => { });

        private static void AssertHealthOutput(string output)
        {
            using var document = JsonDocument.Parse(output);
            Assert.IsTrue(document.RootElement.GetProperty("fever").GetBoolean());
            Assert.IsTrue(document.RootElement.GetProperty("fat").GetBoolean());
        }
    }
}

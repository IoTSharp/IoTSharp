using IoTSharp.Interpreter;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Xunit;

namespace IoTSharp.Test
{
    public class ScriptEngineTest
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void CEngine_CanEvaluateExpression()
        {
            using var loggerFactory = CreateLoggerFactory();
            var engine = new CScriptEngine(loggerFactory.CreateLogger<CScriptEngine>(), CreateOptions());

            var output = engine.Do("2 + 3", "{}");
            Assert.Equal(5, JsonSerializer.Deserialize<int>(output));
        }

        [Fact]
        public void SqlEngine_CanQueryJsonPayload()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new SQLEngine(loggerFactory.CreateLogger<SQLEngine>(), CreateOptions());

            var sql = "select sex from input where (username=\"alice\")";
            var input = "[{\"username\":\"bob\",\"sex\":\"male\"},{\"username\":\"alice\",\"sex\":\"female\"}]";
            var output = engine.Do(sql, input);

            using var document = JsonDocument.Parse(output);
            Assert.Equal("female", document.RootElement[0].GetProperty("sex").GetString());
        }

        [Fact]
        public void SqlEngine_CanQueryJsonPayload2()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new SQLEngine(loggerFactory.CreateLogger<SQLEngine>(), CreateOptions());

            var sql = "select sex from input where username=\"alice\"";
            var input = "[{\"username\":\"bob\",\"sex\":\"male\"},{\"username\":\"alice\",\"sex\":\"female\"}]";
            var output = engine.Do(sql, input);

            using var document = JsonDocument.Parse(output);
            Assert.Equal("female", document.RootElement[0].GetProperty("sex").GetString());
        }

        [Fact]
        public void SqlEngine_CanSelectDeepFieldsAndComputedValues()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new SQLEngine(loggerFactory.CreateLogger<SQLEngine>(), CreateOptions());

            var sql = "select profile.name as name, metrics.score + 5 as total from input where username = \"alice\"";
            var input = """
                [
                  {"username":"bob","profile":{"name":"Bob"},"metrics":{"score":3}},
                  {"username":"alice","profile":{"name":"Alice"},"metrics":{"score":7}}
                ]
                """;

            var output = engine.Do(sql, input);

            using var document = JsonDocument.Parse(output);
            Assert.Equal(12, document.RootElement[0].GetProperty("total").GetInt32());
        }

        [Fact]
        public void SqlEngine_CanSortByUnselectedFieldsWithMultipleColumns()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new SQLEngine(loggerFactory.CreateLogger<SQLEngine>(), CreateOptions());

            var sql = "select username from input order by priority descnum, profile.name asc limit 2";
            var input = """
                [
                  {"username":"charlie","priority":2,"profile":{"name":"Charlie"}},
                  {"username":"alice","priority":3,"profile":{"name":"Alice"}},
                  {"username":"bob","priority":3,"profile":{"name":"Bob"}}
                ]
                """;

            var output = engine.Do(sql, input);

            using var document = JsonDocument.Parse(output);
            Assert.Equal("alice", document.RootElement[0].GetProperty("username").GetString());
        }

        [Fact]
        public void SqlEngine_CanUpdateRowsUsingOrderByAndLimit()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new SQLEngine(loggerFactory.CreateLogger<SQLEngine>(), CreateOptions());

            var sql = "update input set status = \"done\", metrics.score = metrics.score + 1 order by priority descnum, created asc limit 1";
            var input = """
                [
                  {"username":"alice","status":"new","priority":1,"created":2,"metrics":{"score":5}},
                  {"username":"bob","status":"new","priority":3,"created":2,"metrics":{"score":9}},
                  {"username":"charlie","status":"new","priority":3,"created":1,"metrics":{"score":4}}
                ]
                """;

            var output = engine.Do(sql, input);

            using var document = JsonDocument.Parse(output);
            Assert.Equal("done", document.RootElement[2].GetProperty("status").GetString());
        }

        [Fact]
        public void SqlEngine_CanDeleteRowsUsingOrderByAndLimit()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new SQLEngine(loggerFactory.CreateLogger<SQLEngine>(), CreateOptions());

            var sql = "delete from input where status = \"obsolete\" order by created asc limit 1";
            var input = """
                [
                  {"username":"alice","status":"obsolete","created":2},
                  {"username":"bob","status":"obsolete","created":1},
                  {"username":"charlie","status":"active","created":3}
                ]
                """;

            var output = engine.Do(sql, input);

            using var document = JsonDocument.Parse(output);
            Assert.Equal("alice", document.RootElement[0].GetProperty("username").GetString());
        }

        [Fact]
        public void SqlEngine_CanUseInjectedMethods()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new SQLEngine(loggerFactory.CreateLogger<SQLEngine>(), CreateOptions());
            engine.RegisterMethod("tolower", args => args[0]?.ToString()?.ToLowerInvariant());

            var sql = "select tolower(profile.name) as normalized from input where tolower(profile.name) = \"alice\"";
            var input = """
                [
                  {"profile":{"name":"Bob"}},
                  {"profile":{"name":"ALICE"}}
                ]
                """;

            var output = engine.Do(sql, input);

            using var document = JsonDocument.Parse(output);
            Assert.Equal("alice", document.RootElement[0].GetProperty("normalized").GetString());
        }

        [Fact]
        public void SqlEngine_CanInsertRows()
        {
            using var loggerFactory = CreateLoggerFactory();
            using var engine = new SQLEngine(loggerFactory.CreateLogger<SQLEngine>(), CreateOptions());

            var sql = "insert into input set username = \"alice\", profile.name = \"Alice\", metrics.score = 9";
            var input = "[]";
            var output = engine.Do(sql, input);

            using var document = JsonDocument.Parse(output);
            Assert.Equal("Alice", document.RootElement[0].GetProperty("profile").GetProperty("name").GetString());
        }

        [Fact]
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

        [Fact]
        public void BasicEngine_ReturnsInputPayload()
        {
            using var loggerFactory = CreateLoggerFactory();
            var engine = new BASICScriptEngine(loggerFactory.CreateLogger<BASICScriptEngine>(), CreateOptions());

            var input = JsonSerializer.Serialize(new { device = "device-001", enabled = true });
            var output = engine.Do("ignored", input);

            using var document = JsonDocument.Parse(output);
            Assert.Equal("device-001", document.RootElement.GetProperty("device").GetString());
            Assert.True(document.RootElement.GetProperty("enabled").GetBoolean());
        }

        private static IOptions<EngineSetting> CreateOptions()
            => Options.Create(new EngineSetting { Timeout = 4 });

        private static ILoggerFactory CreateLoggerFactory()
            => LoggerFactory.Create(_ => { });

        private static void AssertHealthOutput(string output)
        {
            using var document = JsonDocument.Parse(output);
            Assert.True(document.RootElement.GetProperty("fever").GetBoolean());
            Assert.True(document.RootElement.GetProperty("fat").GetBoolean());
        }
    }
}

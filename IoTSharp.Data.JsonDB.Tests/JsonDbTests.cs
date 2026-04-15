#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using IoTSharp.Data.JsonDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTSharp.Data.JsonDB.Tests
{
    [TestClass]
    public class JsonDbTests
    {
        // ─── Helpers ─────────────────────────────────────────────────────────────────

        private static readonly string SampleUsersJson = """
            [
              {"username":"bob","sex":"male","priority":2,"created":2,"profile":{"name":"Bob"},"metrics":{"score":3},"status":"active"},
              {"username":"alice","sex":"female","priority":3,"created":2,"profile":{"name":"Alice"},"metrics":{"score":7},"status":"active"},
              {"username":"charlie","sex":"male","priority":3,"created":1,"profile":{"name":"Charlie"},"metrics":{"score":4},"status":"obsolete"}
            ]
            """;

        private static JsonDbConnection OpenFromJson(string json)
        {
            var conn = JsonDbConnection.FromJson(json);
            conn.Open();
            return conn;
        }

        // ─── SELECT ──────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Select_ReturnsMatchingRows()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT sex FROM input WHERE username = \"alice\"";
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("female", reader.GetString(0));
            Assert.IsFalse(reader.Read());
        }

        [TestMethod]
        public void Select_WithParens_ReturnsMatchingRows()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT sex FROM input WHERE (username = \"alice\")";
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("female", reader.GetString(0));
        }

        [TestMethod]
        public void Select_Wildcard_ReturnsAllFields()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM input WHERE username = \"bob\"";
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("bob", reader["username"]);
            Assert.AreEqual("male", reader["sex"]);
        }

        [TestMethod]
        public void Select_DeepFieldWithAlias_Works()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT profile.name AS name, metrics.score + 5 AS total FROM input WHERE username = \"alice\"";
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("Alice", reader["name"]);
            Assert.AreEqual(12L, Convert.ToInt64(reader["total"]));
        }

        [TestMethod]
        public void Select_NoWhere_ReturnsAllRows()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT username FROM input";
            var count = 0;
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) count++;

            Assert.AreEqual(3, count);
        }

        // ─── ORDER BY / LIMIT ────────────────────────────────────────────────────────

        [TestMethod]
        public void Select_OrderByDescnumAndAsc_WithLimit()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT username FROM input ORDER BY priority DESCNUM, profile.name ASC LIMIT 2";
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("alice", reader.GetString(0));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("charlie", reader.GetString(0));
            Assert.IsFalse(reader.Read());
        }

        [TestMethod]
        public void Select_LimitOffset_Works()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT username FROM input LIMIT 1, 1";
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("alice", reader.GetString(0));
            Assert.IsFalse(reader.Read());
        }

        // ─── INSERT ──────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Insert_AddsNewRow()
        {
            using var conn = OpenFromJson("[]");
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO input SET username = \"dave\", profile.name = \"Dave\", metrics.score = 9";
            var affected = cmd.ExecuteNonQuery();

            Assert.AreEqual(1, affected);

            var json = conn.GetCurrentJson();
            using var doc = JsonDocument.Parse(json);
            Assert.AreEqual("Dave", doc.RootElement[0].GetProperty("profile").GetProperty("name").GetString());
            Assert.AreEqual(9, doc.RootElement[0].GetProperty("metrics").GetProperty("score").GetInt32());
        }

        // ─── UPDATE ──────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Update_ModifiesMatchingRows()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE input SET status = \"done\" WHERE username = \"alice\"";
            cmd.ExecuteNonQuery();

            var json = conn.GetCurrentJson();
            using var doc = JsonDocument.Parse(json);
            Assert.AreEqual("done", doc.RootElement[1].GetProperty("status").GetString());
            // others unchanged
            Assert.AreEqual("active", doc.RootElement[0].GetProperty("status").GetString());
        }

        [TestMethod]
        public void Update_WithOrderByAndLimit_UpdatesOneRow()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE input SET status = \"done\", metrics.score = metrics.score + 1 ORDER BY priority DESCNUM, created ASC LIMIT 1";
            cmd.ExecuteNonQuery();

            var json = conn.GetCurrentJson();
            using var doc = JsonDocument.Parse(json);
            // charlie has priority=3, created=1 (smallest created among priority=3) → gets updated
            Assert.AreEqual("done", doc.RootElement[2].GetProperty("status").GetString());
            Assert.AreEqual(5, doc.RootElement[2].GetProperty("metrics").GetProperty("score").GetInt32());
        }

        // ─── DELETE ──────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Delete_RemovesMatchingRows()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM input WHERE username = \"bob\"";
            var affected = cmd.ExecuteNonQuery();

            Assert.AreEqual(1, affected);
            var json = conn.GetCurrentJson();
            using var doc = JsonDocument.Parse(json);
            Assert.AreEqual(2, doc.RootElement.GetArrayLength());
        }

        [TestMethod]
        public void Delete_WithOrderByAndLimit_DeletesOneRow()
        {
            var json = """
                [
                  {"username":"alice","status":"obsolete","created":2},
                  {"username":"bob","status":"obsolete","created":1},
                  {"username":"charlie","status":"active","created":3}
                ]
                """;

            using var conn = OpenFromJson(json);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM input WHERE status = \"obsolete\" ORDER BY created ASC LIMIT 1";
            cmd.ExecuteNonQuery();

            var result = conn.GetCurrentJson();
            using var doc = JsonDocument.Parse(result);
            Assert.AreEqual(2, doc.RootElement.GetArrayLength());
            Assert.AreEqual("alice", doc.RootElement[0].GetProperty("username").GetString());
        }

        // ─── Expression evaluation ───────────────────────────────────────────────────

        [TestMethod]
        public void Expression_Arithmetic_Works()
        {
            var json = """[{"x":10,"y":3}]""";
            using var conn = OpenFromJson(json);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT x + y AS add, x - y AS sub, x * y AS mul FROM input";
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(13L, Convert.ToInt64(reader["add"]));
            Assert.AreEqual(7L, Convert.ToInt64(reader["sub"]));
            Assert.AreEqual(30L, Convert.ToInt64(reader["mul"]));
        }

        [TestMethod]
        public void Expression_BooleanLogic_Works()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            // alice: priority=3 (>2), status=active → matches
            // bob: priority=2 (not >2), status=active → excluded
            // charlie: priority=3 (>2), status=obsolete → excluded
            cmd.CommandText = "SELECT username FROM input WHERE status = \"active\" AND priority > 2";
            var count = 0;
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) count++;

            Assert.AreEqual(1, count); // only alice
        }

        [TestMethod]
        public void Expression_NotOperator_Works()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT username FROM input WHERE NOT (username = \"alice\")";
            var names = new List<string>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) names.Add(reader.GetString(0));

            Assert.AreEqual(2, names.Count);
            CollectionAssert.DoesNotContain(names, "alice");
        }

        // ─── Function calls ───────────────────────────────────────────────────────────

        [TestMethod]
        public void FunctionCall_RegisteredMethod_Works()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            conn.RegisterMethod("upper", args => args[0]?.ToString()?.ToUpperInvariant());

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT upper(username) AS uname FROM input WHERE username = \"alice\"";
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("ALICE", reader.GetString(0));
        }

        [TestMethod]
        public void FunctionCall_InWhere_Works()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            conn.RegisterMethod("tolower", args => args[0]?.ToString()?.ToLowerInvariant());

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT tolower(profile.name) AS normalized FROM input WHERE tolower(profile.name) = \"alice\"";
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("alice", reader.GetString(0));
        }

        // ─── ADO ExecuteScalar ───────────────────────────────────────────────────────

        [TestMethod]
        public void ExecuteScalar_ReturnsFirstFieldOfFirstRow()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT sex FROM input WHERE username = \"alice\"";
            var result = cmd.ExecuteScalar();

            Assert.AreEqual("female", result);
        }

        [TestMethod]
        public void ExecuteScalar_ReturnsNullForEmptyResult()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT sex FROM input WHERE username = \"nobody\"";
            var result = cmd.ExecuteScalar();

            Assert.IsNull(result);
        }

        // ─── ADO ExecuteNonQuery ─────────────────────────────────────────────────────

        [TestMethod]
        public void ExecuteNonQuery_Delete_ReturnsAffectedCount()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM input WHERE status = \"obsolete\"";
            var affected = cmd.ExecuteNonQuery();

            Assert.AreEqual(1, affected);
        }

        [TestMethod]
        public void ExecuteNonQuery_Insert_ReturnsAffectedCount()
        {
            using var conn = OpenFromJson("[]");
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO input SET username = \"new\"";
            var affected = cmd.ExecuteNonQuery();

            Assert.AreEqual(1, affected);
        }

        // ─── Parameterized queries ────────────────────────────────────────────────────

        [TestMethod]
        public void ParameterizedQuery_StringParam_Works()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = (JsonDbCommand)conn.CreateCommand();
            cmd.CommandText = "SELECT sex FROM input WHERE username = @name";
            cmd.Parameters.AddWithValue("@name", "alice");
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("female", reader.GetString(0));
        }

        [TestMethod]
        public void ParameterizedQuery_NumericParam_Works()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = (JsonDbCommand)conn.CreateCommand();
            cmd.CommandText = "SELECT username FROM input WHERE priority = @priority";
            cmd.Parameters.AddWithValue("@priority", 3);
            var count = 0;
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) count++;

            Assert.AreEqual(2, count); // alice and charlie
        }

        [TestMethod]
        public void ParameterizedQuery_BoolParam_Works()
        {
            var json = """[{"name":"a","active":true},{"name":"b","active":false}]""";
            using var conn = OpenFromJson(json);
            using var cmd = (JsonDbCommand)conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM input WHERE active = @flag";
            cmd.Parameters.AddWithValue("@flag", true);
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("a", reader.GetString(0));
            Assert.IsFalse(reader.Read());
        }

        // ─── Data sources ─────────────────────────────────────────────────────────────

        [TestMethod]
        public void DataSource_JsonString_Works()
        {
            var json = """[{"id":1,"val":"hello"}]""";
            using var conn = JsonDbConnection.FromJson(json);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT val FROM input WHERE id = 1";
            var result = cmd.ExecuteScalar();

            Assert.AreEqual("hello", result);
        }

        [TestMethod]
        public void DataSource_ConnectionStringJson_Works()
        {
            var json = """[{"id":1,"val":"hello"}]""";
            using var conn = new JsonDbConnection($"Json={json}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT val FROM input";
            var result = cmd.ExecuteScalar();

            Assert.AreEqual("hello", result);
        }

        [TestMethod]
        public void DataSource_JsonNode_IsDeepCloned()
        {
            var original = JsonNode.Parse("""[{"x":1}]""")!;
            using var conn = JsonDbConnection.FromNode(original);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE input SET x = 99";
            cmd.ExecuteNonQuery();

            // Original node should be unchanged
            Assert.AreEqual(1, original[0]!["x"]!.GetValue<int>());

            // Connection's copy should be updated
            var json = conn.GetCurrentJson();
            using var doc = JsonDocument.Parse(json);
            Assert.AreEqual(99, doc.RootElement[0].GetProperty("x").GetInt32());
        }

        [TestMethod]
        public void DataSource_File_Works()
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), $"jsondb_test_{Guid.NewGuid():N}.json");
            try
            {
                File.WriteAllText(tmpFile, """[{"name":"file-test"}]""");

                using var conn = JsonDbConnection.FromFile(tmpFile);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT name FROM input";
                var result = cmd.ExecuteScalar();

                Assert.AreEqual("file-test", result);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        [TestMethod]
        public void DataSource_ConnectionStringFile_Works()
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), $"jsondb_test_{Guid.NewGuid():N}.json");
            try
            {
                File.WriteAllText(tmpFile, """[{"greeting":"hi"}]""");

                using var conn = new JsonDbConnection($"Data Source={tmpFile}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT greeting FROM input";
                var result = cmd.ExecuteScalar();

                Assert.AreEqual("hi", result);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        // ─── Provider factory ─────────────────────────────────────────────────────────

        [TestMethod]
        public void ProviderFactory_CreateConnection_Works()
        {
            var conn = (JsonDbConnection)JsonDbProviderFactory.Instance.CreateConnection()!;
            Assert.IsNotNull(conn);
        }

        [TestMethod]
        public void ProviderFactory_CreateParameter_Works()
        {
            var param = (JsonDbParameter)JsonDbProviderFactory.Instance.CreateParameter()!;
            Assert.IsNotNull(param);
        }

        // ─── Connection string builder ────────────────────────────────────────────────

        [TestMethod]
        public void ConnectionStringBuilder_DataSource_RoundTrips()
        {
            var builder = new JsonDbConnectionStringBuilder { DataSource = "test.json" };
            Assert.IsTrue(builder.ConnectionString.Contains("test.json", StringComparison.OrdinalIgnoreCase));
        }

        // ─── HasRows / FieldCount ─────────────────────────────────────────────────────

        [TestMethod]
        public void Reader_HasRows_FalseWhenEmpty()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM input WHERE username = \"nobody\"";
            using var reader = cmd.ExecuteReader();

            Assert.IsFalse(reader.HasRows);
        }

        [TestMethod]
        public void Reader_FieldCount_MatchesProjectedColumns()
        {
            using var conn = OpenFromJson(SampleUsersJson);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT username, sex FROM input WHERE username = \"alice\"";
            using var reader = cmd.ExecuteReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(2, reader.FieldCount);
            Assert.AreEqual("username", reader.GetName(0));
            Assert.AreEqual("sex", reader.GetName(1));
        }

        // ─── GetCurrentJson ───────────────────────────────────────────────────────────

        [TestMethod]
        public void GetCurrentJson_ReturnsOriginalWhenUnchanged()
        {
            var json = """[{"id":1}]""";
            using var conn = JsonDbConnection.FromJson(json);
            conn.Open();

            var result = conn.GetCurrentJson();
            using var doc = JsonDocument.Parse(result);
            Assert.AreEqual(1, doc.RootElement[0].GetProperty("id").GetInt32());
        }
    }
}

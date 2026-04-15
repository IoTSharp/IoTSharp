# IoTSharp.Data.JsonDB

A lightweight in-memory relational database engine that executes SQL queries over JSON data,
with a full ADO.NET interface.

## Features

- SQL `SELECT`, `INSERT`, `UPDATE`, `DELETE` over JSON arrays and objects
- `WHERE`, `ORDER BY` (with `asc`/`desc`/`ascnum`/`descnum`), `LIMIT`
- Arithmetic and boolean expressions, function registration
- Path-style field access (`profile.name`, `metrics.score`)
- Standard ADO.NET interface: `DbConnection`, `DbCommand`, `DbDataReader`, `DbParameter`
- Parameterized queries (`@paramName`)
- Three data sources: JSON file, JSON string, `System.Text.Json.Nodes.JsonNode`
- `DbProviderFactory` with invariant name `IoTSharp.Data.JsonDB`

## Quick Start

### Connect to a JSON file

```csharp
using var conn = new JsonDbConnection("Data Source=data.json");
conn.Open();

using var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT * FROM input WHERE status = \"active\"";
using var reader = (JsonDbDataReader)cmd.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine(reader["username"]);
}
```

### Connect to an inline JSON string

```csharp
var json = """[{"id":1,"name":"Alice"},{"id":2,"name":"Bob"}]""";
using var conn = JsonDbConnection.FromJson(json);
conn.Open();

using var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT name FROM input WHERE id = 1";
var name = cmd.ExecuteScalar(); // "Alice"
```

### Connect to a `JsonNode`

```csharp
var node = JsonNode.Parse("""[{"x":10},{"x":20}]""")!;
using var conn = JsonDbConnection.FromNode(node); // deep-cloned
conn.Open();

using var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT x FROM input WHERE x > 15";
using var reader = cmd.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine(reader.GetInt32(0)); // 20
}
```

### Parameterized queries

```csharp
using var conn = JsonDbConnection.FromJson(json);
conn.Open();

using var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT * FROM input WHERE username = @name";
cmd.Parameters.AddWithValue("@name", "alice");
using var reader = cmd.ExecuteReader();
```

### Mutating commands and retrieving updated data

```csharp
using var conn = JsonDbConnection.FromJson("""[{"id":1,"status":"new"}]""");
conn.Open();

using var cmd = conn.CreateCommand();
cmd.CommandText = "UPDATE input SET status = \"done\" WHERE id = 1";
var affected = cmd.ExecuteNonQuery(); // 1

// Retrieve modified JSON
var updatedJson = conn.GetCurrentJson();
```

### Registering custom functions

```csharp
conn.RegisterMethod("upper", args => args[0]?.ToString()?.ToUpperInvariant());

cmd.CommandText = "SELECT upper(name) AS name_upper FROM input";
```

### Using the provider factory

```csharp
var factory = JsonDbProviderFactory.Instance;
var conn = (JsonDbConnection)factory.CreateConnection();
conn.ConnectionString = "Data Source=data.json";
conn.Open();
```

### Using the connection string builder

```csharp
var builder = new JsonDbConnectionStringBuilder { DataSource = "data.json" };
using var conn = new JsonDbConnection(builder.ConnectionString);
conn.Open();
```

## Connection String Reference

| Key | Description |
|-----|-------------|
| `Data Source=<path>` | Path to a JSON file (read/write operations update in-memory state only) |
| `Json=<json>` | Inline JSON string (suitable for small payloads) |

For large JSON objects or `JsonNode` instances, use the factory methods directly:
- `JsonDbConnection.FromJson(string json)`
- `JsonDbConnection.FromFile(string path)`
- `JsonDbConnection.FromNode(JsonNode node)`

## SQL Dialect Reference

```sql
-- SELECT
SELECT field1, field2 AS alias, expr + 5 AS computed
FROM input
WHERE condition
ORDER BY field1 ASC, field2 DESCNUM
LIMIT offset, count

-- INSERT
INSERT INTO input SET field1 = value1, nested.field = value2

-- UPDATE
UPDATE input SET field1 = value1, field2 = field2 + 1
WHERE condition
ORDER BY field ASC LIMIT 1

-- DELETE
DELETE FROM input
WHERE condition
ORDER BY field ASC LIMIT 1
```

## Notes

- JsonDB is an **in-memory** engine. File-backed connections load the JSON once on `Open()`;
  write operations (INSERT/UPDATE/DELETE) modify the in-memory root only.
  Call `conn.GetCurrentJson()` to retrieve the modified data.
- Transactions exist for ADO.NET compatibility; `Commit()` is a no-op and `Rollback()` throws.
  Deep-clone your `JsonNode` before opening a connection if you need rollback capability.
- Parameter values of type `string` are safely quoted; numbers and booleans are inlined as literals.

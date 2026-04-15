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
- **Auto-save**: file-backed connections automatically persist INSERT/UPDATE/DELETE back to the source file
- `DataTable` and `DbDataAdapter` support

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

// Retrieve modified JSON (for in-memory connections)
var updatedJson = conn.GetCurrentJson();
```

### Auto-save to JSON file

When a connection is backed by a JSON file, INSERT / UPDATE / DELETE commands
automatically persist changes back to the source file after each execution:

```csharp
using var conn = new JsonDbConnection("Data Source=data.json");
conn.Open();
// AutoSave is true by default

using var cmd = conn.CreateCommand();
cmd.CommandText = "UPDATE input SET status = \"done\" WHERE id = 1";
cmd.ExecuteNonQuery();
// data.json is automatically updated

// Opt out of auto-save
conn.AutoSave = false;
cmd.ExecuteNonQuery();
// data.json is NOT updated until you call:
conn.SaveToFile();

// Check if the connection is file-backed
bool isFileBacked = conn.IsFileBacked; // true
```

### DataTable and DbDataAdapter

```csharp
// Using DbDataAdapter
using var conn = JsonDbConnection.FromJson(json);
conn.Open();
var adapter = new JsonDbDataAdapter("SELECT * FROM input", conn);
var table = new DataTable();
adapter.Fill(table);
// table now contains all rows

// Fill a DataSet
var ds = new DataSet();
adapter.Fill(ds);
// ds.Tables["Table"] contains the results

// Using DataTable.Load with the reader
using var reader = cmd.ExecuteReader();
var dt = new DataTable();
dt.Load(reader);

// Using the static helper for direct reader → DataTable filling
using var reader2 = cmd.ExecuteReader();
var dt2 = new DataTable();
JsonDbDataAdapter.FillFromReader(dt2, reader2);
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
| `Data Source=<path>` | Path to a JSON file. Mutations auto-save back to the file when `AutoSave=true`. |
| `Json=<json>` | Inline JSON string (suitable for small payloads) |

For large JSON objects or `JsonNode` instances, use the factory methods directly:
- `JsonDbConnection.FromJson(string json)`
- `JsonDbConnection.FromFile(string path)`
- `JsonDbConnection.FromNode(JsonNode node)`

## Auto-Save Behaviour

| Data source | `AutoSave = true` (default) | `AutoSave = false` |
|---|---|---|
| JSON file (`Data Source=`) | Writes back after each INSERT/UPDATE/DELETE | No auto-write; call `SaveToFile()` manually |
| JSON string / `FromJson` | No file to write to | No file to write to |
| `JsonNode` / `FromNode` | No file to write to | No file to write to |

- For non-file-backed connections, `SaveToFile()` throws `InvalidOperationException`.
- `GetCurrentJson()` always returns the current in-memory state regardless of persistence mode.
- File writes use a temp-file-then-rename pattern to reduce the risk of data loss.

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

- Transactions exist for ADO.NET compatibility; `Commit()` is a no-op and `Rollback()` throws.
  Deep-clone your `JsonNode` before opening a connection if you need rollback capability.
- Parameter values of type `string` are safely quoted; numbers and booleans are inlined as literals.
- `DataTable.Load(reader)` and `DbDataAdapter.Fill(table)` are both supported; columns are
  auto-created from the first result row.

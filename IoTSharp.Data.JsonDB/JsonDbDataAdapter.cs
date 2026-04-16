#nullable enable

using System;
using System.Data;
using System.Data.Common;

namespace IoTSharp.Data.JsonDB
{
    /// <summary>
    /// Implements <see cref="DbDataAdapter"/> for <see cref="JsonDbConnection"/>.
    /// Supports <c>Fill</c> (populating a <see cref="DataTable"/> or <see cref="DataSet"/>)
    /// and <c>Update</c> (writing <see cref="DataRow"/> changes back via INSERT/UPDATE/DELETE commands).
    /// </summary>
    /// <example>
    /// <code>
    /// using var conn = JsonDbConnection.FromJson(json);
    /// conn.Open();
    ///
    /// var adapter = new JsonDbDataAdapter("SELECT * FROM input", conn);
    /// var table = new DataTable();
    /// adapter.Fill(table);
    /// </code>
    /// </example>
    public sealed class JsonDbDataAdapter : DbDataAdapter
    {
        // ─── Constructors ────────────────────────────────────────────────────────────

        /// <summary>Creates a data adapter with no pre-configured commands.</summary>
        public JsonDbDataAdapter() { }

        /// <summary>Creates a data adapter with a SELECT command text and connection.</summary>
        public JsonDbDataAdapter(string selectCommandText, JsonDbConnection connection)
        {
            SelectCommand = new JsonDbCommand(selectCommandText, connection);
        }

        /// <summary>Creates a data adapter wrapping an existing SELECT command.</summary>
        public JsonDbDataAdapter(JsonDbCommand selectCommand)
        {
            SelectCommand = selectCommand;
        }

        // ─── Commands ────────────────────────────────────────────────────────────────

        /// <summary>The SQL SELECT command used by <see cref="DbDataAdapter.Fill(DataTable)"/>.</summary>
        public new JsonDbCommand? SelectCommand
        {
            get => (JsonDbCommand?)base.SelectCommand;
            set => base.SelectCommand = value;
        }

        /// <summary>The SQL INSERT command used by <see cref="DbDataAdapter.Update(DataTable)"/>.</summary>
        public new JsonDbCommand? InsertCommand
        {
            get => (JsonDbCommand?)base.InsertCommand;
            set => base.InsertCommand = value;
        }

        /// <summary>The SQL UPDATE command used by <see cref="DbDataAdapter.Update(DataTable)"/>.</summary>
        public new JsonDbCommand? UpdateCommand
        {
            get => (JsonDbCommand?)base.UpdateCommand;
            set => base.UpdateCommand = value;
        }

        /// <summary>The SQL DELETE command used by <see cref="DbDataAdapter.Update(DataTable)"/>.</summary>
        public new JsonDbCommand? DeleteCommand
        {
            get => (JsonDbCommand?)base.DeleteCommand;
            set => base.DeleteCommand = value;
        }

        // ─── Fill overrides ─────────────────────────────────────────────────────────

        /// <summary>
        /// Fills a <see cref="DataTable"/> with the results of the <see cref="SelectCommand"/>.
        /// Columns are created automatically from the first result row when the table has
        /// no pre-existing columns.
        /// </summary>
        public new int Fill(DataTable table)
        {
            if (SelectCommand is null)
            {
                throw new InvalidOperationException("SelectCommand must be set before calling Fill.");
            }

            using var reader = SelectCommand.ExecuteReader();
            return FillFromReader(table, reader);
        }

        /// <summary>
        /// Fills the first table (named "Table") in a <see cref="DataSet"/> with the results
        /// of the <see cref="SelectCommand"/>.
        /// </summary>
        public new int Fill(DataSet dataSet)
        {
            if (SelectCommand is null)
            {
                throw new InvalidOperationException("SelectCommand must be set before calling Fill.");
            }

            var table = new DataTable("Table");
            dataSet.Tables.Add(table);
            return Fill(table);
        }

        /// <summary>
        /// Fills the named table in a <see cref="DataSet"/> with the results of the
        /// <see cref="SelectCommand"/>.
        /// </summary>
        public new int Fill(DataSet dataSet, string tableName)
        {
            if (SelectCommand is null)
            {
                throw new InvalidOperationException("SelectCommand must be set before calling Fill.");
            }

            if (!dataSet.Tables.Contains(tableName))
            {
                dataSet.Tables.Add(new DataTable(tableName));
            }

            return Fill(dataSet.Tables[tableName]!);
        }

        /// <summary>
        /// Fills a <see cref="DataTable"/> from an already-open <see cref="JsonDbDataReader"/>.
        /// Useful when you need to share a reader or control the reader lifecycle yourself.
        /// </summary>
        public static int FillFromReader(DataTable table, JsonDbDataReader reader)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(reader);

            var rowsAdded = 0;
            var columnsCreated = table.Columns.Count > 0;

            while (reader.Read())
            {
                if (!columnsCreated)
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var colName = reader.GetName(i);
                        var colType = reader.GetFieldType(i);
                        table.Columns.Add(new DataColumn(colName, colType));
                    }

                    columnsCreated = true;
                }

                var row = table.NewRow();
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    row[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                }

                table.Rows.Add(row);
                rowsAdded++;
            }

            return rowsAdded;
        }

        // ─── Required abstract overrides ─────────────────────────────────────────────

        /// <inheritdoc/>
        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(
            DataRow dataRow,
            IDbCommand? command,
            StatementType statementType,
            DataTableMapping tableMapping)
        {
            return new JsonDbRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        /// <inheritdoc/>
        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(
            DataRow dataRow,
            IDbCommand? command,
            StatementType statementType,
            DataTableMapping tableMapping)
        {
            return new JsonDbRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        /// <inheritdoc/>
        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            if (RowUpdating is { } handler)
            {
                handler(this, (JsonDbRowUpdatingEventArgs)value);
            }
        }

        /// <inheritdoc/>
        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            if (RowUpdated is { } handler)
            {
                handler(this, (JsonDbRowUpdatedEventArgs)value);
            }
        }

        // ─── Events ──────────────────────────────────────────────────────────────────

        /// <summary>Fired before a row is updated in the data source.</summary>
        public event EventHandler<JsonDbRowUpdatingEventArgs>? RowUpdating;

        /// <summary>Fired after a row is updated in the data source.</summary>
        public event EventHandler<JsonDbRowUpdatedEventArgs>? RowUpdated;
    }

    /// <summary>Provides data for the <see cref="JsonDbDataAdapter.RowUpdating"/> event.</summary>
    public sealed class JsonDbRowUpdatingEventArgs : RowUpdatingEventArgs
    {
        internal JsonDbRowUpdatingEventArgs(
            DataRow row,
            IDbCommand? command,
            StatementType statementType,
            DataTableMapping tableMapping)
            : base(row, command, statementType, tableMapping) { }
    }

    /// <summary>Provides data for the <see cref="JsonDbDataAdapter.RowUpdated"/> event.</summary>
    public sealed class JsonDbRowUpdatedEventArgs : RowUpdatedEventArgs
    {
        internal JsonDbRowUpdatedEventArgs(
            DataRow row,
            IDbCommand? command,
            StatementType statementType,
            DataTableMapping tableMapping)
            : base(row, command, statementType, tableMapping) { }
    }
}

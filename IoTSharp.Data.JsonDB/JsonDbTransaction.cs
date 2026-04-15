#nullable enable

using System;
using System.Data;
using System.Data.Common;

namespace IoTSharp.Data.JsonDB
{
    /// <summary>
    /// Minimal DbTransaction implementation for JsonDB.
    /// JsonDB is an in-memory engine that does not support true ACID transactions;
    /// this class exists for ADO.NET compatibility only.
    /// </summary>
    public sealed class JsonDbTransaction : DbTransaction
    {
        private readonly JsonDbConnection _connection;
        private readonly IsolationLevel _isolationLevel;
        private bool _disposed;

        internal JsonDbTransaction(JsonDbConnection connection, IsolationLevel isolationLevel)
        {
            _connection = connection;
            _isolationLevel = isolationLevel;
        }

        public override IsolationLevel IsolationLevel => _isolationLevel;

        protected override DbConnection DbConnection => _connection;

        public override void Commit()
        {
            // No-op: changes are applied immediately to the in-memory root.
        }

        public override void Rollback()
        {
            // Not supported: in-memory mutations cannot be rolled back.
            // Callers that need snapshot/rollback should deep-clone the JsonNode before executing.
            throw new NotSupportedException(
                "JsonDB does not support transaction rollback. " +
                "Deep-clone the JsonNode before executing commands if you need rollback capability.");
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}

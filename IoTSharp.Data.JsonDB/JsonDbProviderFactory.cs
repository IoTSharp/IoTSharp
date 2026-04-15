#nullable enable

using System.Data.Common;

namespace IoTSharp.Data.JsonDB
{
    /// <summary>
    /// Provider factory for creating <see cref="JsonDbConnection"/>, <see cref="JsonDbCommand"/>,
    /// <see cref="JsonDbParameter"/>, and related objects.
    /// </summary>
    /// <remarks>
    /// Provider invariant name: <c>IoTSharp.Data.JsonDB</c>
    /// </remarks>
    public sealed class JsonDbProviderFactory : DbProviderFactory
    {
        /// <summary>Singleton instance.</summary>
        public static readonly JsonDbProviderFactory Instance = new();

        private JsonDbProviderFactory() { }

        public override DbConnection CreateConnection() => new JsonDbConnection();

        public override DbCommand CreateCommand() => new JsonDbCommand();

        public override DbParameter CreateParameter() => new JsonDbParameter();

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
            => new JsonDbConnectionStringBuilder();

        public override bool CanCreateDataAdapter => false;
        public override bool CanCreateCommandBuilder => false;
    }
}

#nullable enable

using System;
using System.Data;
using System.Data.Common;

namespace IoTSharp.Data.JsonDB
{
    /// <summary>
    /// A SQL parameter for use with <see cref="JsonDbCommand"/>.
    /// Parameter names should start with <c>@</c> (e.g. <c>@name</c>).
    /// </summary>
    public sealed class JsonDbParameter : DbParameter
    {
        private string _parameterName = string.Empty;
        private object? _value;
        private DbType _dbType = DbType.Object;
        private ParameterDirection _direction = ParameterDirection.Input;

        /// <inheritdoc/>
        public override DbType DbType
        {
            get => _dbType;
            set => _dbType = value;
        }

        /// <inheritdoc/>
        public override ParameterDirection Direction
        {
            get => _direction;
            set => _direction = value;
        }

        /// <inheritdoc/>
        public override bool IsNullable { get; set; }

        /// <inheritdoc/>
        public override string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value ?? string.Empty;
        }

        /// <inheritdoc/>
        public override string? SourceColumn { get; set; }

        /// <inheritdoc/>
        public override object? Value
        {
            get => _value;
            set => _value = value;
        }

        /// <inheritdoc/>
        public override bool SourceColumnNullMapping { get; set; }

        /// <inheritdoc/>
        public override int Size { get; set; }

        /// <inheritdoc/>
        public override void ResetDbType()
        {
            _dbType = DbType.Object;
        }
    }
}

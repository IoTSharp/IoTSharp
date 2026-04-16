#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

namespace IoTSharp.Data.JsonDB
{
    /// <summary>
    /// Collection of <see cref="JsonDbParameter"/> objects for use with <see cref="JsonDbCommand"/>.
    /// </summary>
    public sealed class JsonDbParameterCollection : DbParameterCollection
    {
        private readonly List<JsonDbParameter> _parameters = new();

        public override int Count => _parameters.Count;

        public override object SyncRoot => ((ICollection)_parameters).SyncRoot;

        public new JsonDbParameter this[int index]
        {
            get => _parameters[index];
            set => _parameters[index] = value;
        }

        public new JsonDbParameter this[string parameterName]
        {
            get
            {
                var idx = IndexOf(parameterName);
                if (idx < 0)
                {
                    throw new ArgumentException($"Parameter '{parameterName}' not found.", nameof(parameterName));
                }

                return _parameters[idx];
            }
            set
            {
                var idx = IndexOf(parameterName);
                if (idx < 0)
                {
                    Add(value);
                }
                else
                {
                    _parameters[idx] = value;
                }
            }
        }

        /// <summary>Adds a parameter with the given name and value.</summary>
        public JsonDbParameter AddWithValue(string parameterName, object? value)
        {
            var param = new JsonDbParameter { ParameterName = NormalizeParameterName(parameterName), Value = value };
            _parameters.Add(param);
            return param;
        }

        public override int Add(object value)
        {
            if (value is not JsonDbParameter param)
            {
                throw new ArgumentException("Value must be a JsonDbParameter.", nameof(value));
            }

            _parameters.Add(param);
            return _parameters.Count - 1;
        }

        public override void AddRange(Array values)
        {
            foreach (var v in values)
            {
                Add(v);
            }
        }

        public override void Clear() => _parameters.Clear();

        public override bool Contains(object value) => value is JsonDbParameter p && _parameters.Contains(p);

        public override bool Contains(string value) => IndexOf(value) >= 0;

        public override void CopyTo(Array array, int index) => ((ICollection)_parameters).CopyTo(array, index);

        public override IEnumerator GetEnumerator() => _parameters.GetEnumerator();

        public override int IndexOf(object value) => value is JsonDbParameter p ? _parameters.IndexOf(p) : -1;

        public override int IndexOf(string parameterName)
        {
            var normalized = NormalizeParameterName(parameterName);
            for (var i = 0; i < _parameters.Count; i++)
            {
                if (string.Equals(NormalizeParameterName(_parameters[i].ParameterName), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        public override void Insert(int index, object value)
        {
            if (value is not JsonDbParameter param)
            {
                throw new ArgumentException("Value must be a JsonDbParameter.", nameof(value));
            }

            _parameters.Insert(index, param);
        }

        public override void Remove(object value)
        {
            if (value is JsonDbParameter param)
            {
                _parameters.Remove(param);
            }
        }

        public override void RemoveAt(int index) => _parameters.RemoveAt(index);

        public override void RemoveAt(string parameterName)
        {
            var idx = IndexOf(parameterName);
            if (idx >= 0)
            {
                _parameters.RemoveAt(idx);
            }
        }

        protected override DbParameter GetParameter(int index) => _parameters[index];

        protected override DbParameter GetParameter(string parameterName)
        {
            var idx = IndexOf(parameterName);
            if (idx < 0)
            {
                throw new ArgumentException($"Parameter '{parameterName}' not found.", nameof(parameterName));
            }

            return _parameters[idx];
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _parameters[index] = (JsonDbParameter)value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var idx = IndexOf(parameterName);
            if (idx < 0)
            {
                _parameters.Add((JsonDbParameter)value);
            }
            else
            {
                _parameters[idx] = (JsonDbParameter)value;
            }
        }

        private static string NormalizeParameterName(string name)
        {
            return name.TrimStart('@');
        }
    }
}

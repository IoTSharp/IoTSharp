using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using IoTSharp.Contracts;
using Microsoft.Extensions.Logging;

namespace IoTSharp.Diagnostics
{
    public class SystemStatusService : IService
    {
        private readonly ConcurrentDictionary<string, Func<object>> _values = new ConcurrentDictionary<string, Func<object>>();

        private readonly ILogger<SystemStatusService> _logger;

        public SystemStatusService(ILogger<SystemStatusService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start()
        {
        }

        public void Set(string uid, object value)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            _values[uid] = () => value;
        }

        public void Set(string uid, Func<object> value)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            if (value == null)
            {
                _values[uid] = null;
            }
            else
            {
                _values[uid] = value;
            }
        }

        public object Get(string uid)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            if (!_values.TryGetValue(uid, out var valueProvider))
            {
                return null;
            }

            return valueProvider();
        }

        public void Delete(string uid)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            _values.TryRemove(uid, out _);
        }

        public Dictionary<string, object> All()
        {
            var result = new Dictionary<string, object>();
            foreach (var value in _values)
            {
                object effectiveValue;

                try
                {
                    effectiveValue = value.Value();
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, $"Error while propagating value for system status '{value.Key}'.");
                    effectiveValue = null;
                }

                result[value.Key] = effectiveValue;
            }

            return result;
        }
    }
}

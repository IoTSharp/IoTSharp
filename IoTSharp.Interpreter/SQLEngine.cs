#nullable enable

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace IoTSharp.Interpreter
{
    /// <summary>
    /// Script engine that executes SQL queries over a JSON payload.
    /// Internally delegates to <see cref="IoTSharp.Data.JsonDB.JsonDbExecutor"/>
    /// from the IoTSharp.Data.JsonDB library.
    /// </summary>
    public sealed class SQLEngine : IDisposable
    {
        private readonly ILogger<SQLEngine> _logger;
        private readonly Dictionary<string, Func<IReadOnlyList<object?>, object?>> _methods =
            new(StringComparer.OrdinalIgnoreCase);

        public SQLEngine(ILogger<SQLEngine> logger, IOptions<EngineSetting> opt)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(opt);
            ArgumentNullException.ThrowIfNull(opt.Value);

            _logger = logger;
        }

        /// <summary>
        /// Registers an external method that can be invoked from SQL expressions.
        /// </summary>
        public void RegisterMethod(string name, Func<IReadOnlyList<object?>, object?> method)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Method name is required.", nameof(name));
            }

            ArgumentNullException.ThrowIfNull(method);
            _methods[name] = method;
        }

        public string Do(string source, string input)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException("SQL source is required.", nameof(source));
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("JSON input is required.", nameof(input));
            }

            var json = IoTSharp.Data.JsonDB.JsonDbExecutor.Execute(source, input, _methods);
            _logger.LogDebug($"source:{Environment.NewLine}{source}{Environment.NewLine}{Environment.NewLine}input:{Environment.NewLine}{input}{Environment.NewLine}{Environment.NewLine} ouput:{Environment.NewLine}{json}{Environment.NewLine}{Environment.NewLine}");
            return json;
        }

        public void Dispose()
        {
        }
    }
}

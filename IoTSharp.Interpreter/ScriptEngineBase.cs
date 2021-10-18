using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Interpreter
{
    public class ScriptEngineBase
    {
        internal   CancellationToken _cancellationToken;
        internal readonly ILogger _logger;
        internal readonly EngineSetting _setting;

        public ScriptEngineBase(ILogger logger, EngineSetting setting, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _logger = logger;
            _setting = setting;
        }
        public virtual void UseCancellationToken(CancellationToken cancellation)
        {
            _cancellationToken = cancellation;
        }
        public virtual string Do(string _source, string input)
        {
            return input;
        }
    }
}

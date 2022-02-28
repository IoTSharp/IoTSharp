using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace IoTSharp.Interpreter
{
    public static class ScriptEnginesExtensions
    {
        public static IServiceCollection AddScriptEngines(this IServiceCollection services, IConfiguration  configuration)
        {
            services.AddTransient<JavaScriptEngine>();
            services.AddTransient<PythonScriptEngine>();
            services.AddTransient<SQLEngine>();
            services.AddTransient<LuaScriptEngine>();
            services.AddTransient<CSharpScriptEngine>();
            services.Configure<EngineSetting>(configuration);
            return services;
        }
    }
}

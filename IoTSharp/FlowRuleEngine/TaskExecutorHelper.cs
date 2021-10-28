using IoTSharp.Data;
using IoTSharp.TaskExecutor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IoTSharp.FlowRuleEngine
{
    public class TaskExecutorHelper
    {

        Dictionary<string, Type> pairs = null;
        Dictionary<string, Type> pairstypename = null;
        private IServiceProvider _sp;
        private ApplicationDbContext _context;

        public TaskExecutorHelper(ILogger<TaskExecutorHelper> logger, IServiceScopeFactory scopeFactor, IOptions<AppSettings> options)
        {
            _sp = scopeFactor.CreateScope().ServiceProvider;
            _context = _sp.GetRequiredService<ApplicationDbContext>();
        }
        public Dictionary<string, Type> GetTaskExecutorList()
        {
            if (pairs == null)
            {
                LoadTypesInfo();
            }
            return pairs;
        }
        public object CreateInstance(string name)
        {
            if (pairs == null)
            {
                LoadTypesInfo();
            }
            return Activator.CreateInstance(pairs[name]);
        }
        public object CreateInstanceByTypeName(string typename)
        {
            if (pairs == null)
            {
                LoadTypesInfo();
            }

            return Activator.CreateInstance(pairstypename[typename]);
        }
        private void LoadTypesInfo()
        {
            pairs = new Dictionary<string, Type>();
            Assembly.GetEntryAssembly().GetTypes().Where(c => c.GetInterfaces().Contains(typeof(ITaskExecutor))).ToList().ForEach(c =>
            {
                pairs.Add(c.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? c.FullName, c);
            });
            typeof(ITaskExecutor).Assembly.GetTypes().Where(c => c.GetInterfaces().Contains(typeof(ITaskExecutor))).ToList().ForEach(c =>
            {
                pairs.Add(c.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? c.FullName, c);
            });
            pairstypename = new Dictionary<string, Type>();
            pairs.Values.ToList().ForEach(t =>
            {
                pairstypename.Add(t.FullName, t);
            });
        }
    }
}


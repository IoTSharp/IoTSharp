using IoTSharp.Data;
using IoTSharp.TaskAction;
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
        public ITaskAction CreateInstance(string name)
        {
            if (pairs == null)
            {
                LoadTypesInfo();
            }
            ITaskAction obj = null;
            if (pairs.TryGetValue(name, out var t))
            {
                obj = CreateInstance(t) as ITaskAction;
                if (obj != null) {
                    obj.ServiceProvider = this._sp;
                }
            }

            return obj;
       
        }
        public ITaskAction CreateInstanceByTypeName(string typename)
        {
            if (pairs == null)
            {
                LoadTypesInfo();
            }
            ITaskAction obj = null;
            if (pairstypename.TryGetValue(typename, out var t))
            {
                obj = CreateInstance(t);
            }
            if (obj != null)
            {
                obj.ServiceProvider = this._sp;
            }
            return obj;
        }

        public ITaskAction CreateInstance(Type t)
        {
            var cnst = t.GetConstructors();
            ITaskAction obj;
            if (cnst.FirstOrDefault()?.GetParameters().Any()==true)
            {
                obj = _sp.GetRequiredService(t) as ITaskAction;
            }
            else
            {
                obj = Activator.CreateInstance(t) as ITaskAction;
            }
            if (obj != null)
            {
                obj.ServiceProvider = this._sp;
            }
            return obj;
        }

        private void LoadTypesInfo()
        {
            pairs = new Dictionary<string, Type>();
            Assembly.GetEntryAssembly().GetTypes().Where(c => c.BaseType== typeof(ITaskAction)).ToList().ForEach(c =>
            {
                var key = c.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? c.FullName;
                if (!pairs.ContainsKey(key))
                {
                    pairs.Add(key, c);
                }
            });
            typeof(ITaskAction).Assembly.GetTypes().Where(c => c.BaseType == typeof(ITaskAction)).ToList().ForEach(c =>
            {
                var key = c.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? c.FullName;
                if (!pairs.ContainsKey(key))
                {
                    pairs.Add(key, c);
                }
            });
            pairstypename = new Dictionary<string, Type>();
            pairs.Values.ToList().ForEach(t =>
            {
                if (!pairstypename.ContainsKey(t.FullName))
                {
                    pairstypename.Add(t.FullName, t);
                }
            });
        }
    }
}


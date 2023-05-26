using System.Reflection;

namespace Quartz
{

    public static class QuartzJobSchedulerExtensions
    {
        public static IServiceCollectionQuartzConfigurator DiscoverJobs(this IServiceCollectionQuartzConfigurator _scheduler) => DiscoverJobs(_scheduler, null);
    
        public static IServiceCollectionQuartzConfigurator DiscoverJobs(this IServiceCollectionQuartzConfigurator  _scheduler , List<Assembly>? additional )
        {
            List<Type>? _jobs = null;
            var types1 = from t in Assembly.GetEntryAssembly()?.GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(QuartzJobSchedulerAttribute), true) select t;
            var types = from t in Assembly.GetCallingAssembly().GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(QuartzJobSchedulerAttribute), true) select t;
            _jobs = new List<Type>();
            _jobs.AddRange(types.ToList());
            _jobs.AddRange(types1.ToList());
            additional?.ForEach(asm =>
            {
                var typeasm = from t in asm.GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(QuartzJobSchedulerAttribute), true) select t;
                _jobs.AddRange(typeasm);
            });
            foreach (var t in _jobs)
            {
                var so = t.GetCustomAttribute<QuartzJobSchedulerAttribute>();
                if (so?.Manual == false)
                {
                    var jobKey = new JobKey(so.Identity ?? t.Name, t.Assembly.GetName().Name);
                    _scheduler.AddJob(t, jobKey, cfg =>
                    {
                        cfg.WithDescription(so.Desciption);

                    });
                    _scheduler.AddTrigger(opts =>
                    {
                        opts.ForJob(jobKey)
                            .WithIdentity(so.TriggerName ?? t.Name)
                            //This Cron interval can be described as "run every minute" (when second is zero)
                            .WithSimpleSchedule(x =>
                            {
                                x.WithInterval(so.WithInterval);
                                if (so.RepeatCount > 0)
                                {
                                    x.WithRepeatCount(so.RepeatCount);

                                }
                                else
                                {
                                    x.RepeatForever();
                                }
                            });
                        if (so.StartAt == DateTimeOffset.MinValue)
                        {
                            opts.StartNow();
                        }
                        else
                        {
                            opts.StartAt(so.StartAt);
                        }
                        if (so.Priority > 0) opts.WithPriority(so.Priority);
                    });
                }
            }
            return _scheduler;
        }
         
    }
  
}
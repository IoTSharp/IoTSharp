using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IoTSharp.Handlers
{
    public class SystemStatusHandler 
    {
        private readonly RuntimeStatusHandler _systemStatusService;

        private readonly ILogger _logger;
        private readonly DateTime _creationTimestamp;

        public SystemStatusHandler(
            RuntimeStatusHandler systemStatusService,
            ILogger<SystemStatusHandler> logger)
        {
            _systemStatusService = systemStatusService ?? throw new ArgumentNullException(nameof(systemStatusService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _creationTimestamp = DateTime.Now;
        }

        public event EventHandler ServicesInitialized;
        public event EventHandler ConfigurationLoaded;
        public event EventHandler StartupCompleted;
        
        public void Start()
        {
            _systemStatusService.Set("startup.timestamp", _creationTimestamp);
            _systemStatusService.Set("startup.duration", null);

            _systemStatusService.Set("framework.description", RuntimeInformation.FrameworkDescription);

            _systemStatusService.Set("process.architecture", RuntimeInformation.ProcessArchitecture);
            _systemStatusService.Set("process.id", Process.GetCurrentProcess().Id);

            _systemStatusService.Set("system.date_time", () => DateTime.Now);
            _systemStatusService.Set("system.processor_count", Environment.ProcessorCount);
            _systemStatusService.Set("system.working_set", () => Environment.WorkingSet);

            _systemStatusService.Set("up_time", () => DateTime.Now - _creationTimestamp);

            _systemStatusService.Set("arguments", string.Join(" ", Environment.GetCommandLineArgs()));

            _systemStatusService.Set("iotsharp.version",  typeof(Startup).Assembly.GetName().Version.ToString());

            AddOSInformation();
            AddThreadPoolInformation();
        }

        public void Reboot(int waitTime)
        {
            _logger.LogInformation("Reboot initiated.");

            Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(waitTime));
                Process.Start("shutdown", " -r now");
            }, CancellationToken.None);
        }

        public void OnServicesInitialized()
        {
            ServicesInitialized?.Invoke(this, EventArgs.Empty);

            _logger.LogInformation("Service startup completed.");
        }

        public void OnConfigurationLoaded()
        {
            ConfigurationLoaded?.Invoke(this, EventArgs.Empty);

            _logger.LogInformation("Configuration loaded.");
        }

        public void OnStartupCompleted()
        {
            _systemStatusService.Set("startup.duration", DateTime.Now - _creationTimestamp);

            PublishBootedNotification();

            StartupCompleted?.Invoke(this, EventArgs.Empty);

            _logger.LogInformation("Startup completed.");
        }

        private void PublishBootedNotification()
        {
            ;
        }

        private void AddOSInformation()
        {
            _systemStatusService.Set("os.description", RuntimeInformation.OSDescription);
            _systemStatusService.Set("os.architecture", RuntimeInformation.OSArchitecture);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _systemStatusService.Set("os.platform", "linux");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _systemStatusService.Set("os.platform", "windows");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _systemStatusService.Set("os.platform", "osx");
            }
        }

        private void AddThreadPoolInformation()
        {
            _systemStatusService.Set("thread_pool.max_worker_threads", () =>
            {
                ThreadPool.GetMaxThreads(out var x, out _);
                return x;
            });

            _systemStatusService.Set("thread_pool.max_completion_port_threads", () =>
            {
                ThreadPool.GetMaxThreads(out _, out var x);
                return x;
            });

            _systemStatusService.Set("thread_pool.min_worker_threads", () =>
            {
                ThreadPool.GetMinThreads(out var x, out _);
                return x;
            });

            _systemStatusService.Set("thread_pool.min_completion_port_threads", () =>
            {
                ThreadPool.GetMinThreads(out _, out var x);
                return x;
            });

            _systemStatusService.Set("thread_pool.available_worker_threads", () =>
            {
                ThreadPool.GetAvailableThreads(out var x, out _);
                return x;
            });

            _systemStatusService.Set("thread_pool.available_completion_port_threads", () =>
            {
                ThreadPool.GetAvailableThreads(out _, out var x);
                return x;
            });
        }
    }
}

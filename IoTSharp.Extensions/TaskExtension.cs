using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    public static class TaskExtension
    {
        public static Task StartSTATask(this TaskFactory task, Action action)
        {
            var tcs = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(new object());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
        public static Task Forget(this Task task)
        {
            return Task.CompletedTask;
        }
    }
}

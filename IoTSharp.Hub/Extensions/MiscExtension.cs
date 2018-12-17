using System.Threading.Tasks;

namespace IoTSharp.Hub.Extensions
{
    public static class MiscExtension
    {
        public static Task Forget(this Task task)
        {
            return Task.CompletedTask;
        }
    }
}
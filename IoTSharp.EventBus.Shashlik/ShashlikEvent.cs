using Shashlik.EventBus;

namespace IoTSharp.EventBus.Shashlik
{
    public class ShashlikEvent<T> : IEvent
    {
        public T? Data { get; set; }
        public static implicit operator ShashlikEvent<T>(T v)
        {
            return new ShashlikEvent<T>() { Data = v };
        }
        public static explicit operator T?(ShashlikEvent<T> v)
        {
            return v.Data;
        }
    }
}

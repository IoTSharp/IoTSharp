using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsSqlSimpleType(this Type type)
        {
            var t = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                t = Nullable.GetUnderlyingType(type);

            return t.IsPrimitive || t.Equals(typeof(string)) ||
                   t.Equals(typeof(DateTime)) || t.Equals(typeof(DateTimeOffset)) || t.Equals(typeof(TimeSpan)) ||
                   t.Equals(typeof(Guid)) ||
                   t.Equals(typeof(byte[])) || t.Equals(typeof(char[]));
        }
        public static bool IsTupleType(this Type type, bool checkBaseTypes = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type == typeof(Tuple))
                return true;
            if (type == typeof(ValueTuple))
                return true;

            while (type != null)
            {
                if (type.IsGenericType)
                {
                    var genType = type.GetGenericTypeDefinition();
                    if (genType == typeof(Tuple<>)
                        || genType == typeof(Tuple<,>)
                        || genType == typeof(Tuple<,,>)
                        || genType == typeof(Tuple<,,,>)
                        || genType == typeof(Tuple<,,,,>)
                        || genType == typeof(Tuple<,,,,,>)
                        || genType == typeof(Tuple<,,,,,,>)
                        || genType == typeof(Tuple<,,,,,,,>)
                        || genType == typeof(Tuple<,,,,,,,>))
                        return true;
                    if (genType == typeof(ValueTuple<>)
                      || genType == typeof(ValueTuple<,>)
                      || genType == typeof(ValueTuple<,,>)
                      || genType == typeof(ValueTuple<,,,>)
                      || genType == typeof(ValueTuple<,,,,>)
                      || genType == typeof(ValueTuple<,,,,,>)
                      || genType == typeof(ValueTuple<,,,,,,>)
                      || genType == typeof(ValueTuple<,,,,,,,>)
                      || genType == typeof(ValueTuple<,,,,,,,>))
                        return true;
                }

                if (!checkBaseTypes)
                    break;

                type = type.BaseType;
            }

            return false;
        }
    }
}

using System;
using System.Reflection;

namespace ArgsToClass
{
    public class ValueParser
    {
        public static object Parse(Type type,string value)
        {
            if (type == typeof(byte))
            {
                return byte.Parse(value);
            }
            else if (type == typeof(sbyte))
            {
                return sbyte.Parse(value);
            }
            else if (type == typeof(char))
            {
                return char.Parse(value);
            }
            else if (type == typeof(short))
            {
                return short.Parse(value);
            }
            else if (type == typeof(ushort))
            {
                return ushort.Parse(value);
            }
            else if (type == typeof(int))
            {
                return int.Parse(value);
            }
            else if (type == typeof(uint))
            {
                return uint.Parse(value);
            }
            else if (type == typeof(long))
            {
                return long.Parse(value);
            }
            else if (type == typeof(ulong))
            {
                return ulong.Parse(value);
            }
            else if (type == typeof(float))
            {
                return float.Parse(value);
            }
            else if (type == typeof(double))
            {
                return double.Parse(value);
            }
            else if (type == typeof(decimal))
            {
                return decimal.Parse(value);
            }
            else if (type == typeof(string))
            {
                return value;
            }
            else if (type == typeof(bool))
            {
                return bool.Parse(value);
            }
            else
            {
                var constructor = type.GetConstructor(BindingFlags.Public, null, new[] {typeof(string)}, null);
                if (constructor != null)
                {
                    return constructor.Invoke(null, new object[] {value});
                }

                var parseMethod = type.GetMethod(
                    "Parse",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new []{typeof(string)},
                    null);
                if (parseMethod != null)
                {
                    return parseMethod.Invoke(null, new object[] {value});
                }

                throw new InvalidOperationException();
            }
        }
    }
}
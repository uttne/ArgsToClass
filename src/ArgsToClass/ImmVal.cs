using System.Collections.Generic;

namespace ArgsToClass
{
    public readonly struct ImmVal<T>
    {
        public readonly T Value;
        public readonly bool HasValue;

        public ImmVal(T value,bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }

        public static implicit operator T (ImmVal<T> x)
        {
            return x.Value;
        }

        public static implicit operator ImmVal<T>(T x)
        {
            return ImmVal.Value(x);
        }

        public override bool Equals(object obj)
        {
            if (obj is ImmVal<T> val)
                return Equals(val);
            return false;
        }

        public bool Equals(ImmVal<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value) && HasValue == other.HasValue;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Value) * 397) ^ HasValue.GetHashCode();
            }
        }
    }

    public static class ImmVal
    {
        public static ImmVal<T> Value<T>(T value)
        {
            return new ImmVal<T>(value, true);
        }
    }
}
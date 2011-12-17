using System;
using System.Diagnostics.Contracts;

namespace VisualProfilerUI.Model.Values
{
    public abstract class Value<TValue> : IValue where TValue : IComparable
    {
        protected readonly TValue ActualValue;
        protected readonly TValue MaxValue;

        protected Value(TValue value, TValue maxValue)
        {
            ActualValue = value;
            MaxValue = maxValue;
        }

        public abstract double ConvertToZeroOneScale();

        public virtual int CompareTo(Value<TValue> other)
        {
            Contract.Ensures(other != null);
            Contract.Ensures(other.GetType() == this.GetType());
            Value<TValue> otherCasted = other as Value<TValue>;

            var compareTo = ActualValue.CompareTo(otherCasted.ActualValue);
            return compareTo;
        }

        public virtual string GetAsString()
        {
            return ActualValue.ToString();
        }

        public int CompareTo(IValue other)
        {
            return CompareTo((Value<TValue>)other);
        }
    }


}
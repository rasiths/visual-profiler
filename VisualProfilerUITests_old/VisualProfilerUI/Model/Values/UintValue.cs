using System;
using System.Diagnostics.Contracts;

namespace VisualProfilerUI.Model.Values
{
    public class UintValue : Value<uint>
    {
        public UintValue(uint value) : base(value)
        {}

        public override double ConvertToZeroOneScale(IValue maxValue)
        {
            Contract.Requires(maxValue != null);
            Contract.Requires(maxValue is UintValue);
            Contract.Ensures(0 <= Contract.Result<double>());
            Contract.Ensures(Contract.Result<double>() <= 1);
            UintValue uintMaxValue = maxValue as UintValue;
            if (uintMaxValue.ActualValue == 0)
                return 0;
            else
                return ActualValue / (double)uintMaxValue.ActualValue;
        }
    }


    public class Uint64Value : Value<UInt64>
    {
        public Uint64Value(UInt64 value)
            : base(value)
        { }

        public override double ConvertToZeroOneScale(IValue maxValue)
        {
            Contract.Requires(maxValue != null);
            Contract.Requires(maxValue is Uint64Value);
            Contract.Ensures(0 <= Contract.Result<double>());
            Contract.Ensures(Contract.Result<double>() <= 1);
            Uint64Value uint64MaxValue = maxValue as Uint64Value;
            if (uint64MaxValue.ActualValue == 0)
                return 0;
            else
                return ActualValue / (double)uint64MaxValue.ActualValue;
        }
    }
}
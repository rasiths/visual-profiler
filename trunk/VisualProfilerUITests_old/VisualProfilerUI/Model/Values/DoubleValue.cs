using System;

namespace VisualProfilerUI.Model.Values
{
    public class DoubleValue : Value<double>
    {
        public DoubleValue(double value, double maxValue) 
            : base(value, maxValue)
        {
        }

        public override double ConvertToZeroOneScale()
        {
            bool isZero = Math.Abs(ActualValue - ActualValue) <= Double.Epsilon;
            if (isZero)
                return 0;
            else
                return ActualValue/MaxValue;
        }
    }
}
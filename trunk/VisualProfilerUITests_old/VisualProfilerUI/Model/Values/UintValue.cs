namespace VisualProfilerUI.Model.Values
{
    public class UintValue : Value<uint>
    {
        public UintValue(uint value, uint maxValue)
            : base(value, maxValue)
        {}

        public override double ConvertToZeroOneScale()
        {
            if (MaxValue == 0)
                return 0;
            else
                return ActualValue/(double)MaxValue;
        }
    }
}
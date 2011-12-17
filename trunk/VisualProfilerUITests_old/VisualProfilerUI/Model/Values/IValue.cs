using System;

namespace VisualProfilerUI.Model.Values
{
    public interface IValue : IComparable<IValue>
    {
        double ConvertToZeroOneScale();
        string GetAsString();
    }
}
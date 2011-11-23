using System;
using System.IO;
using System.Text;

namespace VisualProfilerAccess.ProfilingData.CallTreeElems
{
    public class SamplingCallTreeElem : CallTreeElem<SamplingCallTreeElem>
    {
        protected override void DeserializeFields(Stream byteStream)
        {
            throw new NotImplementedException();
        }

        protected override void ToString(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
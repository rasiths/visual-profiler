using VisualProfilerAccess.ProfilingData.CallTreeElems;

namespace VisualProfilerAccess.ProfilingData.CallTrees
{
    public class SamplingCallTree : CallTree<TracingCallTree, TracingCallTreeElem>
    {
        public override ProfilingDataTypes ProfilingDataType
        {
            get { return ProfilingDataTypes.Sampling;}
        }
    }
}
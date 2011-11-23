using VisualProfilerAccess.ProfilingData.CallTreeElems;

namespace VisualProfilerAccess.ProfilingData.CallTrees
{
    public class TracingCallTree : CallTree<TracingCallTree, TracingCallTreeElem>
    {
        public override ProfilingDataTypes ProfilingDataType
        {
            get { return ProfilingDataTypes.Tracing; }
        }
    }
}

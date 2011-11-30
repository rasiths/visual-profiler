using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTreeElems;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerAccessTests.ProfilingDataTests.CallTreeElemsTests;

namespace VisualProfilerAccessTests.ProfilingDataTests.CallTreesTests
{
    public class MockCallTree : CallTree<MockCallTree, MockCallTreeElem>
    {
        public override ProfilingDataTypes ProfilingDataType
        {
            get { return ProfilingDataTypes.Tracing; } // should be ProfilingDataTypes.Mocking but I did not want to spoil the enum with the strange value. 
        }
    }
}
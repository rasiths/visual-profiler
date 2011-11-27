using System;
using System.IO;
using VisualProfilerAccess.ProfilingData.CallTreeElems;

namespace VisualProfilerAccess.ProfilingData.CallTrees
{
    public class SamplingCallTree : CallTree<SamplingCallTree, SamplingCallTreeElem>
    {
        public UInt64 WallClockDurationHns { get; set; }
        public UInt64 KernelModeDurationHns { get; set; }
        public UInt64 UserModeDurationHns { get; set; }

        public override ProfilingDataTypes ProfilingDataType
        {
            get { return ProfilingDataTypes.Sampling; }
        }

        public override void DeserializeFields(Stream byteStream)
        {
            WallClockDurationHns = byteStream.DeserializeUInt64();
            KernelModeDurationHns = byteStream.DeserializeUInt64();
            UserModeDurationHns = byteStream.DeserializeUInt64();
        }
    }
}
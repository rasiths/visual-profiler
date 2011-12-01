using System;
using System.IO;
using System.Text;
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

        public override void ConvertToString(StringBuilder stringBuilder)
        {
            double userModeSec = UserModeDurationHns / 1e7;
            double kernelModeSec = KernelModeDurationHns / 1e7;
            double wallClockDuration = WallClockDurationHns / 1e7;
            stringBuilder.AppendFormat("Twc={2}s, Tum={0}s, Tkm={1}s", userModeSec, kernelModeSec, wallClockDuration);
        }

        public override void DeserializeFields(Stream byteStream)
        {
            WallClockDurationHns = byteStream.DeserializeUInt64();
            KernelModeDurationHns = byteStream.DeserializeUInt64();
            UserModeDurationHns = byteStream.DeserializeUInt64();
        }
    }
}
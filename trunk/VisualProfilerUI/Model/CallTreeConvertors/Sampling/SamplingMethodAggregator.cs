using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData.CallTreeElems;

namespace VisualProfilerUI.Model.CallTreeConvertors.Sampling
{
    public class SamplingMethodAggregator : MethodAggregator<SamplingCallTreeElem>
    {
       public uint StackTopOccurrenceCount { get; set; }
       public uint LastProfiledFrameInStackCount { get; set; }

        public SamplingMethodAggregator(MethodMetadata methodMd) : base(methodMd)
        {}

        protected override void AggregateElemSpecificFields(SamplingCallTreeElem callTreeElem)
        {
            StackTopOccurrenceCount = callTreeElem.LastProfiledFrameInStackCount;
            LastProfiledFrameInStackCount = callTreeElem.StackTopOccurrenceCount;
        }
    }
}
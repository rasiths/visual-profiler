using System;
using System.Collections.Generic;
using VisualProfilerAccess.ProfilingData.CallTrees;

namespace VisualProfilerAccess.ProfilingData
{
    public class ProfilerDataUpdateEventArgs : EventArgs
    {
        public ProfilerTypes ProfilerType { get; set; }
        public ProfilingDataTypes ProfilingDataType { get; set; }
        public Actions Action { get; set; }
    }

    public class ProfilerDataUpdateEventArgs<TCallTree> : ProfilerDataUpdateEventArgs where TCallTree : CallTree, new()
    {
        public IEnumerable<TCallTree> CallTrees { get; set; }
        public ProfilerAccess<TCallTree> ProfilerAccess { get; set; }
    }
}
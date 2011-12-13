using System.IO;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData.CallTreeElems;

namespace VisualProfilerAccess.ProfilingData.CallTrees
{
    public interface ICallTreeFactory<TCallTree> where TCallTree : CallTree
    {
        TCallTree GetCallTree(Stream byteStream, MetadataCache<MethodMetadata> methodCache);
    }

    class TracingCallTreeFactory : ICallTreeFactory<TracingCallTree>
    {
        public TracingCallTree GetCallTree(Stream byteStream, MetadataCache<MethodMetadata> methodCache)
        {
            TracingCallTreeElemFactory tracingCallTreeElemFactory = new TracingCallTreeElemFactory();
            TracingCallTree tracingCallTree = new TracingCallTree(byteStream, tracingCallTreeElemFactory, methodCache);
            return tracingCallTree;
        }
    }

    class SamplingCallTreeFactory : ICallTreeFactory<SamplingCallTree>
    {
        public SamplingCallTree GetCallTree(Stream byteStream, MetadataCache<MethodMetadata> methodCache)
        {
            SamplingCallTreeElemFactory samplingCallTreeElemFactory = new SamplingCallTreeElemFactory();
            SamplingCallTree samplingCallTree = new SamplingCallTree(byteStream, samplingCallTreeElemFactory, methodCache);
            return samplingCallTree;
        }
    }
}
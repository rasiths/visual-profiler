using System.IO;
using VisualProfilerAccess.Metadata;

namespace VisualProfilerAccess.ProfilingData.CallTreeElems
{
    public interface ICallTreeElemFactory<TCallTreeElem> where TCallTreeElem : CallTreeElem<TCallTreeElem>
    {
        TCallTreeElem GetCallTreeElem(Stream byteStream, MetadataCache<MethodMetadata> methodCache);
    }

    class SamplingCallTreeElemFactory : ICallTreeElemFactory<SamplingCallTreeElem>
    {
        public SamplingCallTreeElem GetCallTreeElem(Stream byteStream, MetadataCache<MethodMetadata> methodCache)
        {
            SamplingCallTreeElem samplingCallTreeElem = new SamplingCallTreeElem(byteStream, this, methodCache);
            return samplingCallTreeElem;
        }
    }

    class TracingCallTreeElemFactory : ICallTreeElemFactory<TracingCallTreeElem>
    {
        public TracingCallTreeElem GetCallTreeElem(Stream byteStream, MetadataCache<MethodMetadata> methodCache)
        {
            TracingCallTreeElem tracingCallTreeElem = new TracingCallTreeElem(byteStream, this, methodCache);
            return tracingCallTreeElem;
        }
    }
}
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using VisualProfilerAccess.ProfilingData.CallTreeElems;

namespace VisualProfilerAccess.ProfilingData.CallTrees
{
    public abstract class CallTree
    {
        public UInt32 ThreadId { get; set; }
        public abstract ProfilingDataTypes ProfilingDataType { get; }

        public virtual void DeserializeFields(Stream byteStream)
        {
        }

        public abstract void Deserialize(Stream byteStream, bool deserializeCallTreeElems = true);
    }

    public abstract class CallTree<TCallTree, TCallTreeElem> : CallTree
        where TCallTree : CallTree<TCallTree, TCallTreeElem>, new()
        where TCallTreeElem : CallTreeElem<TCallTreeElem>, new()
    {
        public TCallTreeElem RootElem { get; set; }

        public override void Deserialize(Stream byteStream, bool deserializeCallTreeElems = true)
        {
            var profilingDataType = (ProfilingDataTypes) byteStream.DeserializeUint32();
            Contract.Assume(ProfilingDataType == profilingDataType,
                            "The profiling data type derived from stream does not match the type's one.");

            ThreadId = byteStream.DeserializeUint32();
            DeserializeFields(byteStream);

            if (deserializeCallTreeElems)
            {
                var callTreeElem = new TCallTreeElem();
                callTreeElem.Deserialize(byteStream, true);
                RootElem = callTreeElem;
            }
        }

        public static TCallTree DeserializeCallTree(Stream byteStream, bool deserializeCallTreeElems = true)
        {
            var callTree = new TCallTree();
            callTree.Deserialize(byteStream, deserializeCallTreeElems);
            return callTree;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Thread Id = {0}, Number of stack divisions = {1}", ThreadId,
                                       RootElem.ChildrenCount);
            stringBuilder.AppendLine();
            RootElem.ConverToString(stringBuilder);

            return stringBuilder.ToString();
        }
    }
}
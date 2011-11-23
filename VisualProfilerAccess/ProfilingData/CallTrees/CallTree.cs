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
        public virtual void Deserialize(Stream byteStream) { }
        public abstract ProfilingDataTypes ProfilingDataType { get; }
       
    }

    public abstract class CallTree<TCallTree, TCallTreeElem> : CallTree
        where TCallTree : CallTree<TCallTree, TCallTreeElem>, new()
        where TCallTreeElem : CallTreeElem<TCallTreeElem>, new()
    {

        public TCallTreeElem RootElem { get; set; }

        public static TCallTree DeserializeCallTree(Stream byteStream, bool deserializeCallTreeElems = true)
        {
            TCallTree callTree = new TCallTree();
            ProfilingDataTypes profilingDataType = (ProfilingDataTypes)byteStream.DeserializeUint32();
            Contract.Assume(callTree.ProfilingDataType == profilingDataType, "The profiling data type derived from stream does not match the type's one.");

            callTree.ThreadId = byteStream.DeserializeUint32();
            callTree.Deserialize(byteStream);

            if (deserializeCallTreeElems)
            {
                TCallTreeElem callTreeElem = new TCallTreeElem();
                callTreeElem.Deserialize(byteStream, true);
                callTree.RootElem = callTreeElem;
            }

            return callTree;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Thread Id = {0}, Number of stack divisions = {1}", ThreadId,
                                       RootElem.ChildrenCount);
            stringBuilder.AppendLine();
            RootElem.ConverToString(stringBuilder);

            return stringBuilder.ToString();
        }
    }
}
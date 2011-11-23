using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualProfilerAccess.ProfilingData
{
    public abstract class CallTree<TCallTree, TCallTreeElem> 
        where TCallTree:CallTree<TCallTree, TCallTreeElem>, new()
        where TCallTreeElem : CallTreeElem<TCallTreeElem>
    {
        public UInt32 ThreadId { get; set; }
        public TCallTreeElem RootElem { get; set; }

        public virtual void Deserialize(Stream byteStream){}

        public static TCallTree DeserializeTree(Stream byteStream)
        {
            TCallTree callTree = new TCallTree();
            callTree.ThreadId = DeserializationUtils.DeserializeUint32(byteStream);
            
            callTree.Deserialize(byteStream);

            callTree.RootElem = new TCallTreeElem();
            callTree.RootElem.Deserialize(byteStream);
            
            return callTree;
        }
    }

    public class TracingCallTree : CallTree<TracingCallTree,TracingCallTreeElem>
    {
       
    }
}

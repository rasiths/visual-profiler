using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualProfilerAccess.ProfilingData
{

    public abstract class CallTreeElem<TTreeElem> where TTreeElem: CallTreeElem<TTreeElem>, new()
    {
        public UInt32 FunctionId { get; set; }
        public TTreeElem[] Children { get; set; }
        public UInt32 ChildrenCount { get; set; }

        public static TTreeElem DeserializeTreeElem(Stream byteStream, bool deserializeChildren = true)
        {
            TTreeElem treeElem = new TTreeElem();
            treeElem.FunctionId = DeserializationUtils.DeserializeUint32(byteStream);
            treeElem.Deserialize(byteStream);
            treeElem.ChildrenCount = DeserializationUtils.DeserializeUint32(byteStream);
            if(deserializeChildren)
            {
                treeElem.Children = new TTreeElem[treeElem.ChildrenCount];
                for (int i = 0; i < treeElem.ChildrenCount; i++)
                {
                    treeElem.Children[i] = DeserializeTreeElem(byteStream);
                }
            }
            return treeElem;
        }

        protected abstract void Deserialize(Stream byteStream);

    }

    public class TracingCallTreeElem : CallTreeElem<TracingCallTreeElem>
    {
        public UInt32 EnterCount { get; set; }
        public UInt32 LeaveCount { get; set; }
        public UInt64 WallClockDurationHns { get; set; }
        public UInt64 KernelModeDurationHns { get; set; }
        public UInt64 UserModeDurationHns { get; set; }


        protected override void Deserialize(Stream byteStream)
        {
            EnterCount = DeserializationUtils.DeserializeUint32(byteStream);
            LeaveCount = DeserializationUtils.DeserializeUint32(byteStream);
            WallClockDurationHns = DeserializationUtils.DeserializeUInt64(byteStream);
            KernelModeDurationHns = DeserializationUtils.DeserializeUInt64(byteStream);
            UserModeDurationHns = DeserializationUtils.DeserializeUInt64(byteStream);
        }
    }
}

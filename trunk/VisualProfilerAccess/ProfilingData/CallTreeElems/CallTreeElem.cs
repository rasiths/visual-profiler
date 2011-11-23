using System;
using System.IO;
using System.Text;

namespace VisualProfilerAccess.ProfilingData.CallTreeElems
{
    public abstract class CallTreeElem
    {
        public UInt32 FunctionId { get; set; }
        public UInt32 ChildrenCount { get; set; }

        protected abstract void DeserializeFields(Stream byteStream);

        public bool IsRootElem()
        {
            bool isRootElem = FunctionId == 0;
            return isRootElem;
        }

        protected abstract void ToString(StringBuilder stringBuilder);
    }

    public abstract class CallTreeElem<TTreeElem> : CallTreeElem
        where TTreeElem : CallTreeElem<TTreeElem>, new()
    {
        public TTreeElem[] Children { get; set; }

        public void Deserialize(Stream byteStream, bool deserializeChildren = true)
        {
            FunctionId = byteStream.DeserializeUint32();
            DeserializeFields(byteStream);
            ChildrenCount = byteStream.DeserializeUint32();
            if (deserializeChildren)
            {
                Children = new TTreeElem[ChildrenCount];
                for (int i = 0; i < ChildrenCount; i++)
                {
                    TTreeElem treeElem = new TTreeElem();
                    Children[i] = treeElem;
                    treeElem.Deserialize(byteStream, true);
                }
            }
        }

        public void ConverToString(StringBuilder stringBuilder, string indentation = "", string indentationChars = "   ")
        {
            if (!IsRootElem())
            {
                stringBuilder.Append(indentation);
                ToString(stringBuilder);
                //stringBuilder.AppendLine();
            }

            int stackDivisionCount = 0;
            foreach (var childTreeElem in Children)
            {
                if (IsRootElem())
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendFormat("-------------- Stack division {0} --------------", stackDivisionCount++);
                }
                stringBuilder.AppendLine();
                childTreeElem.ConverToString(stringBuilder, indentation + indentationChars);
            }

        }
    }
}
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.Values;

namespace VisualProfilerUI.Model.Methods
{
    public abstract class Method //: IMethod
    {
        private readonly IDictionary<Criterion, IValue> _criteriaValuesMap;


        protected Method(
            string name,
            int firstLineNumber,
            int lineExtend)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(firstLineNumber >= 0);
            Contract.Requires(lineExtend > 0);
           
            Name = name;
            FirstLineNumber = firstLineNumber;
            LineExtend = lineExtend;
        }

        public virtual string Name { get; private set; }

        public virtual IEnumerable<Method> CallingMethods { get;  set; }

        public virtual IEnumerable<Method> CalledMethods { get;  set; }

        public int FirstLineNumber { get; private set; }

        public int LineExtend { get; private set; }

        public abstract IValue GetValueFor(Criterion criterion);


    }
}
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using VisualProfilerAccess.SourceLocation;
using VisualProfilerUI.Model.ContainingUnits;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.Model.Values;

namespace VisualProfilerUI.Model
{
    public class Method //: IMethod
    {
        private readonly IDictionary<Criterion, IValue> _criteriaValuesMap;


        public Method(
            string name,
            int firstLineNumber,
            int lineExtend,
            IContainingUnit containingUnit,
            IDictionary<Criterion, IValue> criteriaValuesMap,
            IEnumerable<Method> callingMethods,
            IEnumerable<Method> calledMethods)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(firstLineNumber >= 0);
            Contract.Requires(lineExtend > 0);
            Contract.Requires(containingUnit != null);
            Contract.Requires(criteriaValuesMap != null);
            Contract.Requires(criteriaValuesMap.Count != 0);
            Contract.Requires(callingMethods != null);
            Contract.Requires(calledMethods != null);

            Name = name;
            FirstLineNumber = firstLineNumber;
            LineExtend = lineExtend;
            ContainingUnit = containingUnit;
            _criteriaValuesMap = criteriaValuesMap;
            CallingMethods = callingMethods;
            CalledMethods = calledMethods;

        }

        public Method()
        {

        }

        public virtual string Name { get; private set; }

        public virtual IEnumerable<Method> CallingMethods { get; private set; }

        public virtual IContainingUnit ContainingUnit { get; private set; }

        public virtual IEnumerable<Method> CalledMethods { get; private set; }

        public int FirstLineNumber { get; private set; }

        public int LineExtend { get; private set; }

        public virtual IValue GetValueFor(Criterion criterion)
        {
            var criterionValue = _criteriaValuesMap[criterion];
            return criterionValue;
        }


    }
}

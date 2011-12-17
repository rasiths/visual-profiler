using System.Collections.Generic;
using System.Diagnostics.Contracts;
using VisualProfilerAccess.SourceLocation;
using VisualProfilerUI.Model.ContainingUnits;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.Values;

namespace VisualProfilerUI.Model{
    public class Method //: IMethod
    {
        private readonly IDictionary<Criterion, IValue> _criteriaValuesMap;

        public Method( 
            string name,
            IEnumerable<IMethodLine> methodLines,
            ContainingUnit containingUnit,
            IDictionary<Criterion, IValue> criteriaValuesMap,
            IEnumerable<Method> callingMethods,
            IEnumerable<Method> calledMethods
            )
        {
            _criteriaValuesMap = criteriaValuesMap;
            Contract.Ensures(!string.IsNullOrEmpty(name));
            Contract.Ensures(methodLines != null);
            Contract.Ensures(containingUnit != null);
            Contract.Ensures(criteriaValuesMap != null);
            Contract.Ensures(criteriaValuesMap.Count != 0);
            Contract.Ensures(callingMethods != null);
            Contract.Ensures(calledMethods != null);
           
            Name = name;
            MethodLines = methodLines;
            ContainingUnit = containingUnit;
            CallingMethods = callingMethods;
            CalledMethods = calledMethods;
        }

        public Method()
        {
            
        }

        public virtual  string Name { get; private set; }

        public virtual IEnumerable<Method> CallingMethods { get; private set; }
        
        public virtual ContainingUnit ContainingUnit { get; private set; }

        public virtual IEnumerable<IMethodLine> MethodLines { get; private set; }

        public virtual IEnumerable<Method> CalledMethods { get; private set; }

        public virtual IValue GetValueFor(Criterion criterion)
        {
            var criterionValue = _criteriaValuesMap[criterion];
            return criterionValue;
        }
    }
}

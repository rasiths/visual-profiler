using System;
using System.Collections.Generic;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.Model.Methods;

namespace VisualProfilerUI.Model.ContainingUnits
{
    
    public class SourceFile : ContainingUnit
    {
        public SourceFile(
            ICriteriaContext criteriaContext,
            IEnumerable<Method> containedMethods,
            string name,
            string displayName)
            : base(criteriaContext, containedMethods, name, displayName)
        {
        }

        public SourceFile()
        {
            
        }
    }
}
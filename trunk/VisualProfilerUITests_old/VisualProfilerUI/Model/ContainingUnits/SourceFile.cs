using System.Collections.Generic;

namespace VisualProfilerUI.Model.ContainingUnits
{
    public class SourceFile : ContainingUnit
    {
        public SourceFile(IEnumerable<Method> containedMethods, string name, string displayName) : base(containedMethods, name, displayName)
        {
        }
    }
}
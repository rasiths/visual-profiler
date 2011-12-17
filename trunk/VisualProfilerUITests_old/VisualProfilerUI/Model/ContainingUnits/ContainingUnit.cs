using System.Collections.Generic;

namespace VisualProfilerUI.Model.ContainingUnits
{
   public class ContainingUnit
    {
        public ContainingUnit(
            IEnumerable<Method> containedMethods,
            string name,
            string displayName)
        {
            ContainedMethods = containedMethods;
            DisplayName = name;
            FullName = displayName;
        }

        public IEnumerable<Method> ContainedMethods { get; private set; }
        public string DisplayName { get; private set; }
        public string FullName { get; private set; }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using VisualProfilerUI.Model.ContainingUnits;

namespace VisualProfilerUI.ViewModel
{
    public class VisualProfilerUIViewModel
    {
        // private readonly IEnumerable<ContainingUnit> _containingUnits;

        public ObservableCollection<ContainingUnitViewModel> AllContainingUnits { get; private set; }

        public VisualProfilerUIViewModel(IEnumerable<IContainingUnit> containingUnits)
        {
            Contract.Requires(containingUnits != null);
            var containingUnitViewModels = containingUnits.Select(cu => new ContainingUnitViewModel(cu));
            AllContainingUnits = new ObservableCollection<ContainingUnitViewModel>(containingUnitViewModels);
            
        }
    }
}

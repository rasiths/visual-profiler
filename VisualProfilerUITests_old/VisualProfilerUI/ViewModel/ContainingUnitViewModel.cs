using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using VisualProfilerUI.Model.ContainingUnits;

namespace VisualProfilerUI.ViewModel
{
    public class ContainingUnitViewModel : ViewModelBase
    {
        private IContainingUnit _containingUnit;

        public ContainingUnitViewModel(IContainingUnit containingUnit)
        {
            Contract.Requires(containingUnit != null);
            Contract.Requires(containingUnit.ContainedMethods != null);
            Contract.Requires(containingUnit.ContainedMethods.Count() > 0);
            _containingUnit = containingUnit;
        }

        public IContainingUnit ContainingUnit
        {
            get { return _containingUnit; }
            set
            {
                _containingUnit = value;
                //base.OnPropertyChanged("ContainingUnit");
            }
        }
        public IEnumerable<MethodViewModel> MethodViews
        {
            get
            {
                var methodViewModels = _containingUnit.ContainedMethods.Select(cm => new MethodViewModel(cm));
                return methodViewModels;
            }
        }

        public string Name { get { return _containingUnit.DisplayName; }
       
        }
    }
}

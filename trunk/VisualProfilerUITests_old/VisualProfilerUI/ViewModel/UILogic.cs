using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Windows.Media;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.Model.Methods;
using VisualProfilerUI.Model.Values;
using VisualProfilerUI.View;

namespace VisualProfilerUI.ViewModel
{
    public class UILogic
    {
        public MethodViewModel ActiveMethodVM { get; set; }

        public MethodViewModel HighlightedMethodVM { get; set; }

        public DetailViewModel Detail { get; set; }

        public Dictionary<uint, MethodViewModel> MethodVMByIdDict { get; set; }

        public Dictionary<uint, Method> MethodModelByIdDict { get; set; }

        public Criterion ActiveCriterion { get; set; }

        public ICriteriaContext CriteriaContext { get; set; }

        //public void OnCriteriaChanged(Criterion newCriterion)
        //{
        //    ActiveCriterion = newCriterion;
        //    IValue maxValue = CriteriaContext.GetMaxValueFor(newCriterion);
        //    foreach (var method in MethodModelByIdDict.Values)
        //    {
        //        IValue newValue = method.GetValueFor(newCriterion);
        //        double valueZeroOneScale = newValue.ConvertToZeroOneScale(maxValue);

        //        Tuple<MethodViewModel, MethodViewModel> horVerTuple = HorVerMethodsByIdDict[method.Id];
        //        var horizontalMethod = horVerTuple.Item1;
        //        var verticalMethod = horVerTuple.Item2;


        //        Color adjustedMethodColor = horizontalMethod.FillColorNoAlpha;
        //        adjustedMethodColor.A = (byte) (valueZeroOneScale*byte.MaxValue);
                
        //        SolidColorBrush newBrush = new SolidColorBrush(adjustedMethodColor);
                
        //        horizontalMethod.Fill = newBrush;
        //        verticalMethod.Fill = newBrush;
        //    }

        //    if(ActiveMethodId!=null)
        //    {
        //        ShowMethodInDetail(ActiveMethodId.Value);
        //    }
        //}

        private void ShowMethodInDetail(uint methodId)
        {
            Method method = MethodModelByIdDict[methodId];
            Detail.MethodName = method.Name;
            IValue value = method.GetValueFor(ActiveCriterion);
            Detail.Metrics = value.GetAsString();
        }

        private void ClearDetail()
        {
            Detail.MethodName = string.Empty;
            Detail.Metrics = string.Empty;
        }

        public void MethodActivate(MethodViewModel methodVM)
        {
            if (ActiveMethodVM != null && ActiveMethodVM.IsActive)
            {
                MethodDeactivated(ActiveMethodVM);
            }

            ActiveMethodVM = methodVM;
            methodVM.IsActive = true;
            methodVM.BorderBrush = MethodView.ActiveMethodBorderColor.ToBrush();
            ShowMethodInDetail(methodVM.Id);
        }

        public void MethodDeactivated(MethodViewModel methodVM)
        {
            Contract.Requires(ActiveMethodVM.Id == methodVM.Id);
          
            ActiveMethodVM = null;
            methodVM.BorderBrush = MethodView.MethodBorderColor.ToBrush();
            methodVM.IsActive = false;
        }

        public void InitAllMethodViewModels()
        {
            foreach (var kvp in MethodVMByIdDict)
            {
                uint methodId = kvp.Key;
                MethodViewModel methodViewModel = kvp.Value;
                methodViewModel.Activate += MethodActivate;
                methodViewModel.Deactivate += MethodDeactivated;
                //methodViewModel.Highlight + 
                Method method = MethodModelByIdDict[methodId];
                Contract.Assume(method.Id == methodViewModel.Id);
                IValue activeValue = method.GetValueFor(ActiveCriterion);
                IValue maxValue = CriteriaContext.GetMaxValueFor(ActiveCriterion);
                methodViewModel.Opacity = activeValue.ConvertToZeroOneScale(maxValue);
            }
        }

        public void MethodHighlighted(uint methodId)
        {
            
        }

        public void MethodSuppress(uint methodId)
        {

        }

        

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.Model.Methods;
using VisualProfilerUI.Model.Values;

namespace VisualProfilerUI.ViewModel
{
    public class UILogic
    {
        public Criterion ActiveCriterion { get; set; }

        public uint? ActiveMethodId { get; set; }
        public Color MethodColor { get; set; }
        public Color CalledMethodColor { get; set; }
        public Color CallingMethodColor { get; set; }
               
        public Color BorderColor { get; set; }
        public Color ActiveBorderColor { get; set; }
                    
        public MethodViewModel HorizontalActiveMethod { get; set; }
        public MethodViewModel VerticalActiveMethod { get; set; }

        public DetailViewModel Detail { get; set; }

        public uint? HighlightedMethodId { get; set; }

        public Dictionary<uint, Tuple<MethodViewModel, MethodViewModel>> HorVerMethodsByIdDict { get; set; }

        public Dictionary<uint, Method> MethodModelByIdDict { get; set; }

        public ICriteriaContext CriteriaContext { get; set; }

        public void OnCriteriaChanged(Criterion newCriterion)
        {
            ActiveCriterion = newCriterion;
            IValue maxValue = CriteriaContext.GetMaxValueFor(newCriterion);
            foreach (var method in MethodModelByIdDict.Values)
            {
                IValue newValue = method.GetValueFor(newCriterion);
                double valueZeroOneScale = newValue.ConvertToZeroOneScale(maxValue);

                Tuple<MethodViewModel, MethodViewModel> horVerTuple = HorVerMethodsByIdDict[method.Id];
                var horizontalMethod = horVerTuple.Item1;
                var verticalMethod = horVerTuple.Item2;


                Color adjustedMethodColor = horizontalMethod.FillColorNoAlpha;
                adjustedMethodColor.A = (byte) (valueZeroOneScale*byte.MaxValue);
                
                SolidColorBrush newBrush = new SolidColorBrush(adjustedMethodColor);
                
                horizontalMethod.Fill = newBrush;
                verticalMethod.Fill = newBrush;
            }

            if(ActiveMethodId!=null)
            {
                ShowMethodInDetail(ActiveMethodId.Value);
            }
        }

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

        public void MethodActivate(uint methodId)
        {
            ActiveMethodId = methodId;
            ShowMethodInDetail(methodId);

            Tuple<MethodViewModel, MethodViewModel> horVerTuple = HorVerMethodsByIdDict[methodId];
            var horizontalMethod = horVerTuple.Item1;
            var verticalMethod = horVerTuple.Item2;
            SolidColorBrush activeBorderBrush = new SolidColorBrush(ActiveBorderColor);
            horizontalMethod.BorderBrush = activeBorderBrush;
            verticalMethod.BorderBrush = activeBorderBrush;

        }

        public void MethodDeactivated(uint methodId)
        {
            
            Tuple<MethodViewModel, MethodViewModel> horVerTuple = HorVerMethodsByIdDict[methodId];
            var horizontalMethod = horVerTuple.Item1;
            var verticalMethod = horVerTuple.Item2;
            ClearDetail();
            ActiveMethodId = null;
            SolidColorBrush borderBrush = new SolidColorBrush(BorderColor);
            horizontalMethod.BorderBrush = borderBrush;
            verticalMethod.BorderBrush = borderBrush;
        }

        public void MethodHighlighted(uint methodId)
        {
            
        }

        public void MethodSuppress(uint methodId)
        {

        }

        

    }
}
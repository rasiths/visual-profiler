using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using VisualProfilerUI.Model;
using VisualProfilerUI.Model.Methods;

namespace VisualProfilerUI.ViewModel
{
    public class MethodViewModel : ViewModelBase
    {
        private Method _method;
        private const int LineHeight = 5;
        public MethodViewModel(Method method )
        {
            Contract.Requires(method != null);
            _method = method;
            
        }

        public Brush Fill
        {
            get
            {
                Color color = new Color();
                color.R = (Byte)new Random(DateTime.Now.Millisecond).Next(Byte.MaxValue);
                return new SolidColorBrush(color);
            }
        }

        public int Top
        {
            get
            {
                int top =_method.FirstLineNumber *  LineHeight;
                return top;
            }
        }

        public int Height
        {
            get
            {
                int height = _method.LineExtend*LineHeight;
                return height;
            }
        }
    }
}

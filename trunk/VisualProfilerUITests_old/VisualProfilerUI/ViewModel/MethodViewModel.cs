using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using VisualProfilerUI.Model;

namespace VisualProfilerUI.ViewModel
{
    public class MethodViewModel : ViewModelBase
    {
        private Method _method;

        public MethodViewModel(Method method )
        {
            Contract.Ensures(method !=null);
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
                var firstLine = _method.MethodLines.First();
                var startLineNumber = firstLine.StartLine;
                return startLineNumber;
            }
        }
    }
}

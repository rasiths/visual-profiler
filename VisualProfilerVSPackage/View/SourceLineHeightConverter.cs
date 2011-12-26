using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace VisualProfilerVSPackage.View
{
   
    
   [ValueConversion(typeof(int), typeof(double))]
    public class SourceLineHeightConverter : IValueConverter
   {
       public static double LineHeight = 16;    
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int correctionLineOffset = parameter == null ? 0 : System.Convert.ToInt32(parameter);
            int sourceLinesSpan = System.Convert.ToInt32(value) - correctionLineOffset;
            double linesSpan = sourceLinesSpan * LineHeight;
            return linesSpan;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}

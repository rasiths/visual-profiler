using System.Windows;
using VisualProfilerAccess.ProfilingData;

namespace VisualProfilerUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            visualProfilerUI.Profile(ProfilerTypes.TracingProfiler, @"D:\Honzik\Desktop\Mandelbrot\Mandelbrot\bin\Debug\Mandelbrot.exe");
        }
    }
}

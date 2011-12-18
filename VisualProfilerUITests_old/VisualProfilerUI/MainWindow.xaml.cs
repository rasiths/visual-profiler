using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerUI.Model;
using VisualProfilerUI.Model.ContainingUnits;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.Model.Values;
using VisualProfilerUI.ViewModel;

namespace VisualProfilerUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();




        }

        static object lockObject = new object();

        private static void OnUpdateCallback(object sender, ProfilingDataUpdateEventArgs<TracingCallTree> eventArgs)
        {
            lock (lockObject)
            {
                
                    IEnumerable<TracingCallTree> tracingCallTrees = eventArgs.CallTrees;
                    TracingCallTreeConvertor tracingCallTreeConvertor = new TracingCallTreeConvertor(tracingCallTrees);
                
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            var processStartInfo = new ProcessStartInfo { FileName = @"D:\Honzik\Desktop\Mandelbrot\Mandelbrot\bin\Debug\Mandelbrot.exe" };


            var profilerAccess = new TracingProfilerAccess(
                processStartInfo,
                TimeSpan.FromMilliseconds(2000),
                OnUpdateCallback);

            profilerAccess.StartProfiler();
            profilerAccess.Wait();
        }
    }


}

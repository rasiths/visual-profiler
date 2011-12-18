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

            UplnaBlbost[] uplnaBlbosts = new[]
                                             {
                                                 new UplnaBlbost() {Top = 10, Height = 20},
                                                 new UplnaBlbost() {Top = 40, Height = 20},
                                                 new UplnaBlbost() {Top = 70, Height = 25},
                                             };
            itemControls.ItemsSource = new[]{ uplnaBlbosts};

            // Profile();
        }

        private static void Profile()
        {
            var processStartInfo = new ProcessStartInfo
                                       {FileName = @"D:\Honzik\Desktop\Mandelbrot\Mandelbrot\bin\Debug\Mandelbrot.exe"};


            var profilerAccess = new TracingProfilerAccess(
                processStartInfo,
                TimeSpan.FromMilliseconds(2000),
                OnUpdateCallback);

            profilerAccess.StartProfiler();
        }

        static readonly object LockObject = new object();

        private static int _enter = 0;

        private static void OnUpdateCallback(object sender, ProfilingDataUpdateEventArgs<TracingCallTree> eventArgs)
        {
            if (Interlocked.CompareExchange(ref _enter,0,1) == 1)
            {
                lock (LockObject)
                {
                    IEnumerable<TracingCallTree> tracingCallTrees = eventArgs.CallTrees;
                    TracingCallTreeConvertor tracingCallTreeConvertor = new TracingCallTreeConvertor(tracingCallTrees);
                    _enter = 0;
                }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            _enter = 1;
        }
    }


}

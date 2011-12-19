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

            _uplnaBlbosts = new[]
                               {
                                   new MethodViewModel(1,10, 10, 50){Fill = Brushes.Red, BorderBrush = Brushes.Black, BorderThinkness = 1},
                                   new MethodViewModel(2,25, 30, 30){Fill = Brushes.Blue} ,
                                   new MethodViewModel(3,67,18,99){Fill = Brushes.Green} ,
                               };


            itemControls.ItemsSource = new[] { _uplnaBlbosts };

            // Profile();
        }

        private void Profile()
        {
            var processStartInfo = new ProcessStartInfo { FileName = @"D:\Honzik\Desktop\Mandelbrot\Mandelbrot\bin\Debug\Mandelbrot.exe" };


            var profilerAccess = new TracingProfilerAccess(
                processStartInfo,
                TimeSpan.FromMilliseconds(2000),
                OnUpdateCallback);

            profilerAccess.StartProfiler();
        }

        readonly object LockObject = new object();

        private int _enter = 0;
        private MethodViewModel[] _uplnaBlbosts;

        private void OnUpdateCallback(object sender, ProfilingDataUpdateEventArgs<TracingCallTree> eventArgs)
        {
            if (Interlocked.CompareExchange(ref _enter, 0, 1) == 1)
            {
                lock (LockObject)
                {
                    IEnumerable<TracingCallTree> tracingCallTrees = eventArgs.CallTrees;
                    TracingCallTreeConvertor tracingCallTreeConvertor = new TracingCallTreeConvertor(tracingCallTrees);

                    Dispatcher.BeginInvoke(new Action(() =>
                                                          {
                                                           itemControls.ItemsSource =  tracingCallTreeConvertor.SourceFiles;
                                                          }), null);

                }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //_enter = 1;
            //Console.Beep(3000, 100);
            //MethodViewModel[] uplnaBlbosts = new[]
            //                                 {
            //                                     new MethodViewModel(1,10, 10, 90) ,
            //                                     new MethodViewModel(2,25, 10, 100) ,
            //                                     new MethodViewModel(3,67,18,20) ,
            //                                 };
            //itemControls.ItemsSource = new[] { uplnaBlbosts };

            foreach (var methodViewModel in _uplnaBlbosts)
            {
               methodViewModel.SetMax(200);
            }
        }
    }


}

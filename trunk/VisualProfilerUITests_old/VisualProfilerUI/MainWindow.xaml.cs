using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerUI.Model;
using VisualProfilerUI.Model.ContainingUnits;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.Model.Methods;
using VisualProfilerUI.Model.Values;
using VisualProfilerUI.ViewModel;

namespace VisualProfilerUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _uiLogic = new UILogic();
            _uiLogic.ActiveCriterion = TracingCriteriaContext.CallCountCriterion;


            var criterionSwitchVMs = new[] {
                new CriterionSwitchViewModel(TracingCriteriaContext.CallCountCriterion){IsActive = true},
                new CriterionSwitchViewModel(TracingCriteriaContext.TimeActiveCriterion),
                new CriterionSwitchViewModel(TracingCriteriaContext.TimeWallClockCriterion)};
            _uiLogic.CriterionSwitchVMs = criterionSwitchVMs;



            foreach (var switchVM in criterionSwitchVMs)
            {
                switchVM.CriterionChanged += _uiLogic.ActivateCriterion;
            }

            criteriaSwitch.DataContext = criterionSwitchVMs;

            Profile();
        }

        private void Profile()
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo { FileName = @"D:\Honzik\Desktop\Mandelbrot\Mandelbrot\bin\Debug\Mandelbrot.exe" };

            var profilerAccess = new TracingProfilerAccess(
                processStartInfo,
                TimeSpan.FromMilliseconds(1000),
                OnUpdateCallback);

            profilerAccess.StartProfiler();
        }

        readonly object LockObject = new object();

        private int _enter = 0;

        private UILogic _uiLogic;

        private void OnUpdateCallback(object sender, ProfilingDataUpdateEventArgs<TracingCallTree> eventArgs)
        {
            //  if (Interlocked.CompareExchange(ref _enter, 0, 1) == 1)
            {
                lock (LockObject)
                {

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var treeConvertor = new TracingCallTreeConvertor(eventArgs.CallTrees);

                        // DetailViewModel detailViewModel = new DetailViewModel();

                        var containingUnitViewModels = treeConvertor.SourceFiles.Select(sf =>
                        {
                            var methodViewModels = sf.ContainedMethods.Select(cm => new MethodViewModel(cm));
                            var containingUnitViewModel = new ContainingUnitViewModel(
                                System.IO.Path.GetFileName(sf.FullName));
                            containingUnitViewModel.Height = sf.Height;
                            containingUnitViewModel.MethodViewModels = methodViewModels.OrderBy(mvm => mvm.Top).ToArray();
                            return containingUnitViewModel;
                        }).ToArray();


                        List<MethodViewModel> allMethodViewModels = new List<MethodViewModel>();
                        foreach (var containingUnitViewModel in containingUnitViewModels)
                        {
                            allMethodViewModels.AddRange(containingUnitViewModel.MethodViewModels);
                        }



                        _uiLogic.CriteriaContext = treeConvertor.CriteriaContext;
                        _uiLogic.MethodModelByIdDict = treeConvertor.MethodDictionary.ToDictionary(kvp => kvp.Key.Id,
                                                                                                  kvp => kvp.Value);
                        _uiLogic.MethodVMByIdDict = allMethodViewModels.ToDictionary(kvp => kvp.Id, kvp => kvp);
                        var detailViewModel = new DetailViewModel();
                        detail.DataContext = detailViewModel;
                        _uiLogic.Detail = detailViewModel;
                        _uiLogic.InitAllMethodViewModels();

                        containingUnits.ItemsSource = containingUnitViewModels;
                        containingUnits.DataContext = treeConvertor.MaxEndLine + 20;
                        var sortedMethodVMs = new ObservableCollection<MethodViewModel>(_uiLogic.MethodVMByIdDict.Values);

                        sortedMethods.DataContext = sortedMethodVMs;
                        _uiLogic.SortedMethodVMs = sortedMethodVMs;
                        _uiLogic.ActivateCriterion(_uiLogic.ActiveCriterion);
                        //  unitsScrollView.Height = treeConvertor.MaxEndLine + 20;
                    }), null);

                }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _enter = 1;

        }
    }


}

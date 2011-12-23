﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerUI.Model.CallTreeConvertors;
using VisualProfilerUI.Model.CallTreeConvertors.Sampling;
using VisualProfilerUI.Model.CallTreeConvertors.Tracing;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.ViewModel;

namespace VisualProfilerUI
{
    public partial class VisualProfilerUIView : UserControl
    {
        public VisualProfilerUIView()
        {
            InitializeComponent();
            Profile(ProfilerTypes.TracingProfiler);
        }

        public void Profile(ProfilerTypes profiler)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo { FileName = @"D:\Honzik\Desktop\Mandelbrot\Mandelbrot\bin\Debug\Mandelbrot.exe" };
            CriterionSwitchViewModel[] criterionSwitchVMs;
            _uiLogic = new UILogic();
            
            if (profiler == ProfilerTypes.TracingProfiler)
            {
                var profilerAccess = new TracingProfilerAccess(
                    processStartInfo,
                    TimeSpan.FromMilliseconds(1000),
                    OnUpdateCallback);
                profilerAccess.StartProfiler();

                _uiLogic.ActiveCriterion = TracingCriteriaContext.CallCountCriterion;


                criterionSwitchVMs = new[] {
                new CriterionSwitchViewModel(TracingCriteriaContext.CallCountCriterion){IsActive = true},
                new CriterionSwitchViewModel(TracingCriteriaContext.TimeActiveCriterion),
                new CriterionSwitchViewModel(TracingCriteriaContext.TimeWallClockCriterion)};
                ;
            }
            else
            {
                var profilerAccess = new SamplingProfilerAccess(
                    processStartInfo,
                    TimeSpan.FromMilliseconds(1000),
                    OnUpdateCallback);
                profilerAccess.StartProfiler();

                _uiLogic.ActiveCriterion = SamplingCriteriaContext.TopStackOccurrenceCriteria;
                 criterionSwitchVMs = new[] {
                new CriterionSwitchViewModel(SamplingCriteriaContext.TopStackOccurrenceCriteria){IsActive = true},
                new CriterionSwitchViewModel(SamplingCriteriaContext.DurationCriteria)};

            }


            foreach (var switchVM in criterionSwitchVMs)
            {
                switchVM.CriterionChanged += _uiLogic.ActivateCriterion;
            }

            criteriaSwitch.DataContext = criterionSwitchVMs;
            _uiLogic.CriterionSwitchVMs = criterionSwitchVMs;

        }

        readonly object LockObject = new object();

        private int _enter = 0;

        private UILogic _uiLogic;

        private void OnUpdateCallback(object sender, ProfilingDataUpdateEventArgs<TracingCallTree> eventArgs)
        {
            if (Interlocked.CompareExchange(ref _enter, 0, 1) == 1)
            {
                lock (LockObject)
                {
                    CallTreeConvertor treeConvertor = new TracingCallTreeConvertor(eventArgs.CallTrees);
                    SetupUI(treeConvertor);
                }
            }
        }

        private void OnUpdateCallback(object sender, ProfilingDataUpdateEventArgs<SamplingCallTree> eventArgs)
        {
            //if (Interlocked.CompareExchange(ref _enter, 0, 1) == 1)
            {
                lock (LockObject)
                {
                    CallTreeConvertor treeConvertor = new SamplingCallTreeConvertor(eventArgs.CallTrees);
                    SetupUI(treeConvertor);
                }
            }
        }

        private void SetupUI(CallTreeConvertor treeConvertor)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
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
            }), null);


        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _enter = 1;

        }
    }


}

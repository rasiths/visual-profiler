using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerUI.Model.CallTreeConvertors;
using VisualProfilerUI.Model.CallTreeConvertors.Sampling;
using VisualProfilerUI.Model.CallTreeConvertors.Tracing;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.ViewModel;

namespace VisualProfilerUI.View
{
    public partial class VisualProfilerUIView : UserControl
    {
        private int _enter = 0;
        private readonly UILogic _uiLogic;
        readonly object _lockObject = new object();

        public VisualProfilerUIView()
        {
            if (Application.ResourceAssembly == null)
            {
                Application.ResourceAssembly = typeof (MainWindow).Assembly;
            }
            InitializeComponent();
            _uiLogic = new UILogic();
        }

        public UILogic UILogic
        {
            get { return _uiLogic; }
        }

        public void Profile(ProfilerTypes profiler, string processPath)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo { FileName = processPath };
            CriterionSwitchViewModel[] criterionSwitchVMs;

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
            }
            else
            {
                var profilerAccess = new SamplingProfilerAccess(
                    processStartInfo,
                    TimeSpan.FromMilliseconds(1000),
                    OnUpdateCallback);
                profilerAccess.StartProfiler();

                _uiLogic.ActiveCriterion = SamplingCriteriaContext.TopStackOccurrenceCriterion;

                criterionSwitchVMs = new[] {
                new CriterionSwitchViewModel(SamplingCriteriaContext.TopStackOccurrenceCriterion){IsActive = true},
                new CriterionSwitchViewModel(SamplingCriteriaContext.DurationCriterion)};
            }

            foreach (var switchVM in criterionSwitchVMs)
            {
                switchVM.CriterionChanged += _uiLogic.ActivateCriterion;
            }

            criteriaSwitch.DataContext = criterionSwitchVMs;
            _uiLogic.CriterionSwitchVMs = criterionSwitchVMs;
        }

        private void OnUpdateCallback(object sender, ProfilingDataUpdateEventArgs<TracingCallTree> eventArgs)
        {
           //if (Interlocked.CompareExchange(ref _enter, 0, 1) == 1)
            {
                lock (_lockObject)
                {
                    CallTreeConvertor treeConvertor = new TracingCallTreeConvertor(eventArgs.CallTrees);
                    UpdateUI(treeConvertor);
                }
            }
        }

        private void OnUpdateCallback(object sender, ProfilingDataUpdateEventArgs<SamplingCallTree> eventArgs)
        {
            //if (Interlocked.CompareExchange(ref _enter, 0, 1) == 1)
            {
                lock (_lockObject)
                {
                    CallTreeConvertor treeConvertor = new SamplingCallTreeConvertor(eventArgs.CallTrees);
                    UpdateUI(treeConvertor);
                }
            }
        }

        private void UpdateUI(CallTreeConvertor treeConvertor)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var containingUnitViewModels = treeConvertor.SourceFiles.Select(sf =>
                {
                    var methodViewModels = sf.ContainedMethods.Select(cm => new MethodViewModel(cm));
                    var containingUnitViewModel = new ContainingUnitViewModel(sf.FullName);
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
                containingUnits.DataContext = new { Height = treeConvertor.MaxEndLine + 20 };

                OnDataUpdate(containingUnitViewModels);

                var sortedMethodVMs = new ObservableCollection<MethodViewModel>(_uiLogic.MethodVMByIdDict.Values);
                sortedMethods.DataContext = sortedMethodVMs;
                _uiLogic.SortedMethodVMs = sortedMethodVMs;
                _uiLogic.ActivateCriterion(_uiLogic.ActiveCriterion);
            }), null);
        }

        public event Action<IEnumerable<ContainingUnitViewModel>> DataUpdate;

        public void OnDataUpdate(IEnumerable<ContainingUnitViewModel> data)
        {
            Action<IEnumerable<ContainingUnitViewModel>> handler = DataUpdate;
            if (handler != null) handler(data);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _enter = 1;
        }
    }


}

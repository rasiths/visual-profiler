using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData.CallTreeElems;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerUI.Model.ContainingUnits;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.Criteria.TracingCriteria;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.Model.Methods;
using VisualProfilerUI.Model.Values;

namespace VisualProfilerUI.Model
{
    public class TracingCallTreeConvertor
    {
        private IEnumerable<SourceFile> _sourceFiles;

        public TracingCallTreeConvertor(IEnumerable<TracingCallTree> tracingCallTrees)
        {

            UInt64 totalActiveTime = 0;
            List<TracingCallTreeElem> flattenedTreeList = new List<TracingCallTreeElem>();
            foreach (var callTree in tracingCallTrees)
            {
                totalActiveTime = callTree.UserModeDurationHns + callTree.KernelModeDurationHns;
                FlattenCallTree(callTree.RootElem, flattenedTreeList);
            }

            var aggregators = flattenedTreeList.GroupBy(
                elem => elem.MethodMetadata).Select(
                grouping =>
                {
                    MethodMetadata methodMetadata = grouping.Key;
                    var aggregator = new TracingCallTreeElemAggregator(methodMetadata);
                    aggregator.AggregateRange(grouping);
                    return aggregator;
                });

            // TracingCallTreeElemAggregator[] tracingCallTreeElemAggregators = aggregators.ToArray();
            Dictionary<MethodMetadata, Method> methodDictionary = aggregators.Select(agr =>
                {
                    double activeTime = agr.CycleTime * totalActiveTime /
                               (double)TracingCallTreeElemAggregator.TotalCycleTime;

                    Method method = new TracingMethod(
                        agr.MethodMd.Name,
                        agr.MethodMd.GetSourceLocations().First().StartLine,
                        agr.MethodMd.GetSourceLocations().Last().EndLine,
                        new UintValue(agr.EnterCount),
                        new Uint64Value(agr.WallClockDurationHns),
                        new DoubleValue(activeTime));

                    return new KeyValuePair<MethodMetadata, Method>(agr.MethodMd, method);
                }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


            foreach (var agr in aggregators)
            {
                Method method = methodDictionary[agr.MethodMd];
                method.CalledMethods = methodDictionary.Where(kvp =>
                    agr.CalledFunctions.Contains(kvp.Key)).Select(kvp => kvp.Value).ToArray();
                method.CallingMethods = methodDictionary.Where(kvp =>
                   agr.CallingFunctions.Contains(kvp.Key)).Select(kvp => kvp.Value).ToArray();
            }


            double maxActiveTime = TracingCallTreeElemAggregator.TotalCycleTime * totalActiveTime / (double)
                                                                                   TracingCallTreeElemAggregator.TotalCycleTime;
            TracingCriteriaContext context = new TracingCriteriaContext(
                new UintValue(TracingCallTreeElemAggregator.MaxEnterCount),
                new Uint64Value(TracingCallTreeElemAggregator.MaxWallClockDurationHns),
                new DoubleValue(maxActiveTime));

            _sourceFiles = methodDictionary.GroupBy(kvp => kvp.Key.GetSourceFilePath()).Select(kvp =>
                new SourceFile(context,
                               kvp.Select(k => k.Value).ToArray(),
                               kvp.Key,
                               Path.GetFileName(kvp.Key))).ToArray();

         
        }

        private void FlattenCallTree(TracingCallTreeElem rootElem, List<TracingCallTreeElem> flattenedTreeList)
        {
            flattenedTreeList.AddRange(rootElem.Children);
            foreach (var callTreeElem in rootElem.Children)
            {
                FlattenCallTree(callTreeElem, flattenedTreeList);
            }
        }


    }

    public class TracingCallTreeElemAggregator
    {
        [ThreadStatic]
        private static ulong _maxWallClockDurationHns;

        [ThreadStatic]
        private static uint _maxLeaveCount;

        [ThreadStatic]
        private static uint _maxEnterCount;

        [ThreadStatic]
        private static ulong _totalCycleTime;

        [ThreadStatic]
        private static ulong _maxCycleTime;

        public uint FunctionId { get; set; }
        public UInt32 EnterCount { get; set; }
        public UInt32 LeaveCount { get; set; }
        public UInt64 WallClockDurationHns { get; set; }
        public UInt64 CycleTime { get; set; }
        public MethodMetadata MethodMd { get; set; }

        public HashSet<MethodMetadata> CallingFunctions { get; set; }
        public HashSet<MethodMetadata> CalledFunctions { get; set; }

        public static UInt64 TotalCycleTime
        {
            get { return _totalCycleTime; }
            set { _totalCycleTime = value; }
        }

        public static UInt32 MaxEnterCount
        {
            get { return _maxEnterCount; }
            set { _maxEnterCount = value; }
        }

        public static UInt32 MaxLeaveCount
        {
            get { return _maxLeaveCount; }
            set { _maxLeaveCount = value; }
        }

        public static UInt64 MaxWallClockDurationHns
        {
            get { return _maxWallClockDurationHns; }
            set { _maxWallClockDurationHns = value; }
        }

        public static UInt64 MaxCycleTime
        {
            get { return _maxCycleTime; }
            set { _maxCycleTime = value; }
        }

        public TracingCallTreeElemAggregator(MethodMetadata methodMd)
        {
            CallingFunctions = new HashSet<MethodMetadata>();
            CalledFunctions = new HashSet<MethodMetadata>();
            FunctionId = methodMd.Id;
            MethodMd = methodMd;
        }

        public void Aggregate(TracingCallTreeElem callTreeElem)
        {
            Contract.Requires(FunctionId == callTreeElem.FunctionId);
            EnterCount += callTreeElem.EnterCount;
            LeaveCount += callTreeElem.LeaveCount;
            WallClockDurationHns += callTreeElem.WallClockDurationHns;
            CycleTime += callTreeElem.CycleTime;
            TotalCycleTime += callTreeElem.CycleTime;

            MaxEnterCount = Math.Max(MaxEnterCount, callTreeElem.EnterCount);
            MaxLeaveCount = Math.Max(MaxLeaveCount, callTreeElem.LeaveCount);
            WallClockDurationHns = Math.Max(WallClockDurationHns, callTreeElem.WallClockDurationHns);
            MaxCycleTime = Math.Max(MaxCycleTime, callTreeElem.CycleTime);

            if (!callTreeElem.ParentElem.IsRootElem())
                CallingFunctions.Add(callTreeElem.ParentElem.MethodMetadata);

            var methodMetadata = callTreeElem.Children.Select(cte => cte.MethodMetadata);
            foreach (var childMethodMetadata in methodMetadata)
            {
                CalledFunctions.Add(childMethodMetadata);
            }
        }

        public void AggregateRange(IEnumerable<TracingCallTreeElem> callTreeElems)
        {
            foreach (var callTreeElem in callTreeElems)
            {
                Aggregate(callTreeElem);
            }
        }
    }
}

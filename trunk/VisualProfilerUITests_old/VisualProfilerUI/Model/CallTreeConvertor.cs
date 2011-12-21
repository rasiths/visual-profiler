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
using VisualProfilerAccess.SourceLocation;
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
        private readonly IEnumerable<SourceFile> _sourceFiles;
        private readonly Dictionary<MethodMetadata, Method> _methodDictionary;

        public TracingCallTreeConvertor(IEnumerable<TracingCallTree> tracingCallTrees)
        {

            UInt64 globalAggregatedActiveTime = 0;
            List<TracingCallTreeElem> flattenedTreeList = new List<TracingCallTreeElem>();
            foreach (var callTree in tracingCallTrees)
            {
                globalAggregatedActiveTime = callTree.UserModeDurationHns + callTree.KernelModeDurationHns;
                FlattenCallTree(callTree.RootElem, flattenedTreeList);
            }

            GlobalAggregatedValues globalAggregatedValues = new GlobalAggregatedValues();
            var aggregators = flattenedTreeList.GroupBy(
                elem => elem.MethodMetadata).Select(
                grouping =>
                {
                    MethodMetadata methodMetadata = grouping.Key;
                    var aggregator = new TracingMethodAggregator(methodMetadata, globalAggregatedValues);
                    aggregator.AggregateRange(grouping);
                    return aggregator;
                });

            int maxEndLine = 0;
            _methodDictionary = aggregators.Select(agr =>
            {
                double activeTime = agr.CycleTime * globalAggregatedActiveTime /
                                    (double)globalAggregatedValues.TotalCycleTime;
                int startLine;
                int endLine;
                bool isConstructor = agr.MethodMd.Name.EndsWith("ctor");
                if (isConstructor)
                    FindConstructorBody(agr.MethodMd, out startLine, out endLine);
                else
                {
                    startLine = agr.MethodMd.GetSourceLocations().First().StartLine;
                    endLine = agr.MethodMd.GetSourceLocations().Last().EndLine;
                }
                maxEndLine = Math.Max(maxEndLine, endLine);

                Method method = new TracingMethod(
                    agr.FunctionId,
                    agr.MethodMd.Name,
                    startLine,
                    endLine - startLine + 1,
                    new UintValue(agr.EnterCount),
                    new Uint64Value(agr.WallClockDurationHns),
                    new DoubleValue(activeTime));

                return new KeyValuePair<MethodMetadata, Method>(agr.MethodMd, method);
            }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


            foreach (var agr in aggregators)
            {
                Method method = _methodDictionary[agr.MethodMd];
                method.CalledMethods = _methodDictionary.Where(kvp =>
                    agr.CalledFunctions.Contains(kvp.Key)).Select(kvp => kvp.Value).ToArray();
                method.CallingMethods = _methodDictionary.Where(kvp =>
                   agr.CallingFunctions.Contains(kvp.Key)).Select(kvp => kvp.Value).ToArray();
            }


            UintValue maxCallCount = new UintValue(uint.MinValue);
            Uint64Value maxWallClockDuration = new Uint64Value(uint.MinValue);
            DoubleValue maxActiveTime = new DoubleValue(double.MinValue);
            foreach (var method in _methodDictionary.Values)
            {
                maxCallCount = (UintValue)Max(method.GetValueFor(TracingCriteriaContext.CallCountCriterion), maxCallCount);
                maxWallClockDuration = (Uint64Value)Max(method.GetValueFor(TracingCriteriaContext.TimeWallClockCriterion), maxWallClockDuration);
                maxActiveTime = (DoubleValue)Max(method.GetValueFor(TracingCriteriaContext.TimeActiveCriterion), maxActiveTime);

            }

            

            CriteriaContext = new TracingCriteriaContext(
                                     maxCallCount,
                                     maxWallClockDuration,
                                     maxActiveTime);

            _sourceFiles = _methodDictionary.GroupBy(kvp => kvp.Key.GetSourceFilePath()).Select(kvp =>
                new SourceFile(CriteriaContext,
                               kvp.Select(k => k.Value).ToArray(),
                               kvp.Key,
                               Path.GetFileName(kvp.Key),
                               maxEndLine
                               )).ToArray();


        }

        private void FindConstructorBody(MethodMetadata method, out int startLine, out int endline)
        {
            IMethodLine openingBrace = method.GetSourceLocations().FirstOrDefault(sl => sl.EndIndex - sl.StartIndex == 1);
            IMethodLine closingBrace = method.GetSourceLocations().LastOrDefault(sl => sl.EndIndex - sl.StartIndex == 1);
            if (openingBrace != null && closingBrace != null)
            {
                startLine = openingBrace.StartLine;
                endline = closingBrace.EndLine;
            }
            else
            {
                startLine = method.GetSourceLocations().First().StartLine;
                endline = method.GetSourceLocations().Last().EndLine;
            }
        }

        private IValue Max(IValue first, IValue second)
        {
            if (first.CompareTo(second) >= 0)
            {
                return first;
            }
            else
            {
                return second;
            }

        }

        public IEnumerable<SourceFile> SourceFiles
        {
            get { return _sourceFiles; }
        }

        public Dictionary<MethodMetadata, Method> MethodDictionary
        {
            get { return _methodDictionary; }
        }

        public TracingCriteriaContext CriteriaContext { get; set; }

        private void FlattenCallTree(TracingCallTreeElem rootElem, List<TracingCallTreeElem> flattenedTreeList)
        {
            flattenedTreeList.AddRange(rootElem.Children);
            foreach (var callTreeElem in rootElem.Children)
            {
                FlattenCallTree(callTreeElem, flattenedTreeList);
            }
        }


    }

    public class GlobalAggregatedValues
    {
        public ulong TotalCycleTime { get; set; }
        public ulong TotalActiveTime { get; set; }
    }

    public class TracingMethodAggregator
    {
        public uint FunctionId { get; set; }
        public UInt32 EnterCount { get; set; }
        public UInt32 LeaveCount { get; set; }
        public UInt64 WallClockDurationHns { get; set; }
        public UInt64 CycleTime { get; set; }
        public MethodMetadata MethodMd { get; set; }

        public HashSet<MethodMetadata> CallingFunctions { get; set; }
        public HashSet<MethodMetadata> CalledFunctions { get; set; }

        public GlobalAggregatedValues GlobalAggregatedValues { get; set; }

        public TracingMethodAggregator(MethodMetadata methodMd, GlobalAggregatedValues globalAggregatedValues)
        {
            CallingFunctions = new HashSet<MethodMetadata>();
            CalledFunctions = new HashSet<MethodMetadata>();
            FunctionId = methodMd.Id;
            MethodMd = methodMd;
            GlobalAggregatedValues = globalAggregatedValues;
        }

        public void Aggregate(TracingCallTreeElem callTreeElem)
        {

            Contract.Requires(FunctionId == callTreeElem.FunctionId);
            EnterCount += callTreeElem.EnterCount;
            LeaveCount += callTreeElem.LeaveCount;
            WallClockDurationHns += callTreeElem.WallClockDurationHns;
            CycleTime += callTreeElem.CycleTime;
            GlobalAggregatedValues.TotalCycleTime += callTreeElem.CycleTime;
         

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

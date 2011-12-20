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

            UInt64 totalActiveTime = 0;
            List<TracingCallTreeElem> flattenedTreeList = new List<TracingCallTreeElem>();
            foreach (var callTree in tracingCallTrees)
            {
                totalActiveTime = callTree.UserModeDurationHns + callTree.KernelModeDurationHns;
                FlattenCallTree(callTree.RootElem, flattenedTreeList);
            }

            MaxAndAggregatedValues maxAndAggregatedValues = new MaxAndAggregatedValues();
            var aggregators = flattenedTreeList.GroupBy(
                elem => elem.MethodMetadata).Select(
                grouping =>
                {
                    MethodMetadata methodMetadata = grouping.Key;
                    var aggregator = new TracingCallTreeElemAggregator(methodMetadata, maxAndAggregatedValues);
                    aggregator.AggregateRange(grouping);
                    return aggregator;
                });

            // TracingCallTreeElemAggregator[] tracingCallTreeElemAggregators = aggregators.ToArray();
            _methodDictionary = aggregators.Select(agr =>
            {
                double activeTime = agr.CycleTime * totalActiveTime /
                                    (double)maxAndAggregatedValues.TotalCycleTime;

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





            double maxActiveTime = maxAndAggregatedValues.TotalCycleTime * totalActiveTime / (double)
                                                                                   maxAndAggregatedValues.TotalCycleTime;
            CriteriaContext = new TracingCriteriaContext(
                new UintValue(maxAndAggregatedValues.MaxEnterCount),
                new Uint64Value(maxAndAggregatedValues.MaxWallClockDurationHns),
                new DoubleValue(maxActiveTime));

            _sourceFiles = _methodDictionary.GroupBy(kvp => kvp.Key.GetSourceFilePath()).Select(kvp =>
                new SourceFile(CriteriaContext,
                               kvp.Select(k => k.Value).ToArray(),
                               kvp.Key,
                               Path.GetFileName(kvp.Key))).ToArray();

         
        }

        private void FindConstructorBody(MethodMetadata method, out int startLine, out int endline)
        {
            IMethodLine openingBrace = method.GetSourceLocations().FirstOrDefault(sl => sl.EndIndex - sl.StartIndex == 1);
            IMethodLine closingBrace = method.GetSourceLocations().LastOrDefault(sl => sl.EndIndex - sl.StartIndex == 1);
            if(openingBrace != null && closingBrace != null )
            {
                startLine = openingBrace.StartLine;
                endline = closingBrace.EndLine;
            }else
            {
                startLine = method.GetSourceLocations().First().StartLine;
                endline = method.GetSourceLocations().Last().EndLine;
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

    public class MaxAndAggregatedValues
    {
        public ulong TotalCycleTime { get; set; }
        public uint MaxEnterCount { get; set; }
        public uint MaxLeaveCount { get; set; }
        public ulong MaxWallClockDurationHns { get; set; }
        public ulong MaxCycleTime { get; set; }
    }

    public class TracingCallTreeElemAggregator
    {
        public uint FunctionId { get; set; }
        public UInt32 EnterCount { get; set; }
        public UInt32 LeaveCount { get; set; }
        public UInt64 WallClockDurationHns { get; set; }
        public UInt64 CycleTime { get; set; }
        public MethodMetadata MethodMd { get; set; }

        public HashSet<MethodMetadata> CallingFunctions { get; set; }
        public HashSet<MethodMetadata> CalledFunctions { get; set; }

        public MaxAndAggregatedValues MaxAndAggregatedValues { get; set; }

        public TracingCallTreeElemAggregator(MethodMetadata methodMd, MaxAndAggregatedValues maxAndAggregatedValues)
        {
            CallingFunctions = new HashSet<MethodMetadata>();
            CalledFunctions = new HashSet<MethodMetadata>();
            FunctionId = methodMd.Id;
            MethodMd = methodMd;
            MaxAndAggregatedValues = maxAndAggregatedValues;
        }

        public void Aggregate(TracingCallTreeElem callTreeElem)
        {
            
            Contract.Requires(FunctionId == callTreeElem.FunctionId);
            EnterCount += callTreeElem.EnterCount;
            LeaveCount += callTreeElem.LeaveCount;
            WallClockDurationHns += callTreeElem.WallClockDurationHns;
            CycleTime += callTreeElem.CycleTime;
            MaxAndAggregatedValues.TotalCycleTime += callTreeElem.CycleTime;

            MaxAndAggregatedValues.MaxEnterCount = Math.Max(MaxAndAggregatedValues.MaxEnterCount, callTreeElem.EnterCount);
            MaxAndAggregatedValues.MaxLeaveCount = Math.Max(MaxAndAggregatedValues.MaxLeaveCount, callTreeElem.LeaveCount);
            WallClockDurationHns = Math.Max(WallClockDurationHns, callTreeElem.WallClockDurationHns);
            MaxAndAggregatedValues.MaxCycleTime = Math.Max(MaxAndAggregatedValues.MaxCycleTime, callTreeElem.CycleTime);

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

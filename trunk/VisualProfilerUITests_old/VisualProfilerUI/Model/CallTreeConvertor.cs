using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData.CallTreeElems;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.Model.Values;

namespace VisualProfilerUI.Model
{
    public class TracingCallTreeConvertor
    {
        public TracingCallTreeConvertor(TracingCallTree tracingCallTree)
        {
            List<TracingCallTreeElem> flattenedTreeList = new List<TracingCallTreeElem>();
            FlattenCallTree(tracingCallTree.RootElem, flattenedTreeList);
            
            var aggregators = flattenedTreeList.GroupBy(
                elem => elem.MethodMetadata).Select(
                grouping =>
                {
                    var aggregator = new TracingCallTreeElemAggregator(grouping.Key);
                    aggregator.AggregateRange(grouping);
                    return aggregator;
                });
            IDictionary<Criterion, IValue> criteriaValuesMap = new Dictionary<Criterion, IValue>();

            IEnumerable<Method> enumerable = aggregators.Select(agr =>
                                                            {
                                                                Method method = new Method(
                                                                    agr.MethodMd.Name,
                                                                    agr.MethodMd.GetSourceLocations().First().StartLine,
                                                                    agr.MethodMd.GetSourceLocations().Last().StartLine,
                                                                    criteriaValuesMap);
                                                                return new KeyValuePair<uint, Method>(method.);
                                                            });
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
        public uint FunctionId { get; set; }
        public UInt32 EnterCount { get; set; }
        public UInt32 LeaveCount { get; set; }
        public UInt64 WallClockDurationHns { get; set; }
        public UInt64 CycleTime { get; set; }
        public MethodMetadata MethodMd { get; set; }

        public HashSet<uint> CallingFunctions { get; set; }
        public HashSet<uint> CalledFunctions { get; set; }

        public TracingCallTreeElemAggregator(MethodMetadata methodMd)
        {
            CallingFunctions = new HashSet<uint>();
            CalledFunctions = new HashSet<uint>();
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

            if (!callTreeElem.ParentElem.IsRootElem())
                CallingFunctions.Add(callTreeElem.ParentElem.FunctionId);

            IEnumerable<uint> childFuncIds = callTreeElem.Children.Select(cte => cte.FunctionId);
            foreach (var childFuncId in childFuncIds)
            {
                CalledFunctions.Add(childFuncId);
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

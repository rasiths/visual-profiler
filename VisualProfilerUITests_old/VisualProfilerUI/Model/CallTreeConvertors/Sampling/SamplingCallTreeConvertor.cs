using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData.CallTreeElems;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerUI.Model.ContainingUnits;
using VisualProfilerUI.Model.CriteriaContexts;
using VisualProfilerUI.Model.Methods;
using VisualProfilerUI.Model.Values;

namespace VisualProfilerUI.Model.CallTreeConvertors.Sampling
{
    public class SamplingCallTreeConvertor : CallTreeConvertor
    {
        private readonly IEnumerable<SamplingMethodAggregator> _aggregators;
        private readonly SamplingGlobalAggregatedValues _globalAggregatedValues;

        public SamplingCallTreeConvertor(IEnumerable<SamplingCallTree> tracingCallTrees)
        {
            _globalAggregatedValues = new SamplingGlobalAggregatedValues();

            var flattenedTreeList = new List<SamplingCallTreeElem>();
            foreach (SamplingCallTree callTree in tracingCallTrees)
            {
                _globalAggregatedValues.TotalActiveTime += callTree.UserModeDurationHns + callTree.KernelModeDurationHns;
                _globalAggregatedValues.WallClockDurationHns += callTree.WallClockDurationHns;
                FlattenCallTree(callTree.RootElem, flattenedTreeList);
            }

            _aggregators = flattenedTreeList.GroupBy(
                elem => elem.MethodMetadata).Select(
                    grouping =>
                        {
                            MethodMetadata methodMetadata = grouping.Key;
                            var aggregator = new SamplingMethodAggregator(methodMetadata);
                            aggregator.AggregateRange(grouping);
                            return aggregator;
                        });

            _globalAggregatedValues.LastProfiledFrameInStackCount = 0;
            _globalAggregatedValues.StackTopOccurrenceCount = 0;

            foreach (var methodAgr in _aggregators)
            {
                _globalAggregatedValues.LastProfiledFrameInStackCount += methodAgr.LastProfiledFrameInStackCount;
                _globalAggregatedValues.StackTopOccurrenceCount += methodAgr.StackTopOccurrenceCount;
            }

            CreateMethodByMetadataDictionary();

            // InterconnectMethodCalls(aggregators);


            CreateCriteriaContext();

            PopulateSourceFiles();
        }

        protected void CreateCriteriaContext()
        {
            var maxTopStackOccurrence = new UintValue(uint.MinValue);
            var maxDuration = new DoubleValue(uint.MinValue);
            
            foreach (Method method in _methodDictionary.Values)
            {
                maxTopStackOccurrence =
                    (UintValue) Max(method.GetValueFor(SamplingCriteriaContext.TopStackOccurrenceCriteria), maxTopStackOccurrence);
                maxDuration =
                    (DoubleValue)
                    Max(method.GetValueFor(SamplingCriteriaContext.DurationCriteria), maxDuration);
            }

            CriteriaContext = new SamplingCriteriaContext(
                maxTopStackOccurrence,
                maxDuration);
        }

        protected  void CreateMethodByMetadataDictionary()
        {
            _maxEndLine = 0;
            _methodDictionary = _aggregators.Select(
                methodAgr =>
                    {
                        double duration = (methodAgr.StackTopOccurrenceCount+methodAgr.LastProfiledFrameInStackCount)*
                                            _globalAggregatedValues.WallClockDurationHns/
                                            (double) (_globalAggregatedValues.StackTopOccurrenceCount+_globalAggregatedValues.LastProfiledFrameInStackCount);
                        ;
                        int startLine;
                        int endLine;
                        bool isConstructor = methodAgr.MethodMd.Name.EndsWith("ctor");
                        if (isConstructor)
                            FindConstructorBody(methodAgr.MethodMd, out startLine,
                                                out endLine);
                        else
                        {
                            startLine =
                                methodAgr.MethodMd.GetSourceLocations().First().StartLine;
                            endLine = methodAgr.MethodMd.GetSourceLocations().Last().EndLine;
                        }
                        _maxEndLine = Math.Max(_maxEndLine, endLine);

                        Method method = new SamplingMethod(
                            methodAgr.FunctionId,
                            methodAgr.MethodMd.Name,
                            startLine,
                            endLine - startLine + 1,
                            methodAgr.MethodMd.Class.Name,
                            methodAgr.MethodMd.GetSourceFilePath(),
                            new UintValue(methodAgr.StackTopOccurrenceCount + methodAgr.LastProfiledFrameInStackCount),
                            new DoubleValue(duration));

                        return new KeyValuePair<MethodMetadata, Method>(methodAgr.MethodMd,
                                                                        method);
                    }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.Criteria.SamplingCriteria;
using VisualProfilerUI.Model.Values;

namespace VisualProfilerUI.Model.CriteriaContexts
{
    public class SamplingCriteriaContext : ICriteriaContext
    {
        private readonly UintValue _maxTopStackOccurrence;
        private readonly DoubleValue _maxDuration;
        private readonly Criterion[] _availableCriteria;
        public static readonly TopStackOccurrenceCriteria TopStackOccurrenceCriteria = new TopStackOccurrenceCriteria();
        public static readonly DurationCriteria DurationCriteria = new DurationCriteria();

        public SamplingCriteriaContext(UintValue maxTopStackOccurrence, DoubleValue maxDuration)
        {
            Contract.Requires(maxTopStackOccurrence != null);
            Contract.Requires(maxDuration != null);
            _maxTopStackOccurrence = maxTopStackOccurrence;
            _maxDuration = maxDuration;
            _availableCriteria = new Criterion[]{TopStackOccurrenceCriteria, DurationCriteria};
        }

        public IEnumerable<Criterion> AvailableCriteria
        {
            get { return _availableCriteria; }
        }

        public IValue GetMaxValueFor(Criterion criterion)
        {
            if (criterion == TopStackOccurrenceCriteria)
                return _maxTopStackOccurrence;

            if (criterion == DurationCriteria)
                return _maxDuration;

            var exceptionMessage = string.Format("Criterion of type {0} is not available in {1}", criterion.GetType().Name,
                                                 this.GetType().Name);
            throw new ArgumentException(exceptionMessage);
        }
    }
}
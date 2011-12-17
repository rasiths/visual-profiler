using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.Criteria.TracingCriteria;
using VisualProfilerUI.Model.Values;

namespace VisualProfilerUI.Model.CriteriaContexts
{
    public class TracingCriteriaContext : ICriteriaContext
    {
        private readonly UintValue _maxCallCount;
        private readonly DoubleValue _maxTimeWallClock;
        private readonly DoubleValue _maxTimeActive;
        private readonly Criterion[] _availableCriteria;
        private readonly CallCountCriterion _callCountCriterion = new CallCountCriterion();
        private readonly TimeWallClockCriterion _timeWallClockCriterion = new TimeWallClockCriterion();
        private readonly TimeActiveCriterion _timeActiveCriterion = new TimeActiveCriterion();

        public TracingCriteriaContext(UintValue maxCallCount, DoubleValue maxTimeWallClock, DoubleValue maxTimeActive)
        {
            Contract.Ensures(maxCallCount != null);
            Contract.Ensures(maxTimeWallClock != null);
            Contract.Ensures(maxTimeActive != null);
            _maxCallCount = maxCallCount;
            _maxTimeWallClock = maxTimeWallClock;
            _maxTimeActive = maxTimeActive;
            _availableCriteria = new Criterion[] { _callCountCriterion, _timeWallClockCriterion, _timeActiveCriterion };
        }

        public IEnumerable<Criterion> AvailableCriteria
        {
            get { return _availableCriteria; }
        }

        public IValue GetMaxValueFor(Criterion criterion)
        {
            if (criterion == _callCountCriterion)
                return _maxCallCount;
            if (criterion == _timeWallClockCriterion)
                return _maxTimeWallClock;
            if (criterion == _timeActiveCriterion)
                return _maxTimeActive;

            var exceptionMessage = string.Format("Criterion of type {0} is not available in {1}", criterion.GetType().Name,
                                                 this.GetType().Name);
            throw new ArgumentException(exceptionMessage);

        }
    }
}
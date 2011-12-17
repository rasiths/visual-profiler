using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using VisualProfilerUI.Model;
using VisualProfilerUI.Model.ContainingUnits;
using VisualProfilerUI.Model.Criteria;
using VisualProfilerUI.Model.Criteria.SamplingCriteria;
using VisualProfilerUI.Model.Criteria.TracingCriteria;
using VisualProfilerUI.Model.Values;

namespace VisualProfilerUITests.ModelTests
{
    [TestFixture]
    public class MethodTest
    {
        private Method _method;
        private UintValue _callCountValue;
        private DoubleValue _durationValue;

        [TestFixtureSetUp]
        public void SetUp()
        {
            Mock<IContainingUnit> mockContainingUnit = new Mock<IContainingUnit>(MockBehavior.Strict);
            var criteriaValuesMap = new Dictionary<Criterion, IValue>();
            _durationValue = new DoubleValue(50);
            criteriaValuesMap.Add(new DurationCriteria(), _durationValue);
            _callCountValue = new UintValue(500);
            criteriaValuesMap.Add(new CallCountCriterion(), _callCountValue);
            Mock<IEnumerable<Method>> mockCallingMethods = new Mock<IEnumerable<Method>>(MockBehavior.Strict);
            Mock<IEnumerable<Method>> mockCalledMethods = new Mock<IEnumerable<Method>>(MockBehavior.Strict);
            _method = new Method("stub", 20, 50, mockContainingUnit.Object, criteriaValuesMap, mockCallingMethods.Object,
                                 mockCalledMethods.Object);
            
        }

        [Test]
        public void GetValueForTest()
        {
            IValue durationValue = _method.GetValueFor(new DurationCriteria());
            
            Assert.AreEqual(_durationValue, durationValue);
            IValue callCountValue = _method.GetValueFor(new CallCountCriterion());
            Assert.AreEqual(_callCountValue, callCountValue);

        }
    }
}

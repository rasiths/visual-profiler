﻿using System;
using System.IO;
using NUnit.Framework;
using VisualProfilerAccess.ProfilingData.CallTreeElems;
using VisualProfilerAccessTests.MetadataTests;

namespace VisualProfilerAccessTests.ProfilingDataTests.CallTreeElemsTests
{
    [TestFixture]
    public class TracingCallTreeElemTest
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _memoryStream = _multipleElemBytes.ConvertToMemoryStream();
            _rootElem = new TracingCallTreeElem();
            _rootElem.Deserialize(_memoryStream);
        }

        #endregion

        private readonly byte[] _singleElemBytes = {
                                                       0x34, 0x34, 0x16, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
                                                       0x00, 0x62, 0x46,
                                                       0xC9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0x00,
                                                       0x23, 0x24, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00,
                                                       0x00
                                                   };

        private UInt32 ExpectedFunctionId = 0x163434;
        private UInt32 ExpectedEnterCount = 1;
        private UInt32 ExpectedLeaveCount = 1;
        private UInt64 ExpectedWallClockDurationHns = 0xc94662;
        private UInt64 ExpectedKernelModeDurationHns;
        private UInt64 ExpectedUserModeDurationHns = 0x72423;
        private UInt32 ExpectedChildrenCount = 3;


        /* TestData represented by _multipleElemBytes
0x143434:[TestAssembly]TestNamespace.TestClass.Main(args),Twc=ababd4s,Tum=4c2c2s,Tkm=0s,Ec=1,Lc=1
   0x143440:[TestAssembly]TestNamespace.TestClass.MessageThatCallsOtherMethod(),Twc=9febd7s,Tum=72423s,Tkm=0s,Ec=2,Lc=2
      0x14344c:[TestAssembly]TestNamespace.TestClass.OtherMethod(),Twc=633014s,Tum=665b4bs,Tkm=0s,Ec=14,Lc=14
      0x143458:[TestAssembly]TestNamespace.TestClass.TestMessageWith2Arguments(testArgumentA, testArgumentB),Twc=331eafs,Tum=31fcf5s,Tkm=0s,Ec=a,Lc=a
   0x14344c:[TestAssembly]TestNamespace.TestClass.OtherMethod(),Twc=3d09fs,Tum=26161s,Tkm=0s,Ec=1,Lc=1
   0x143458:[TestAssembly]TestNamespace.TestClass.TestMessageWith2Arguments(testArgumentA, testArgumentB),Twc=3f7afs,Tum=4c2c2s,Tkm=0s,Ec=1,Lc=1
       */

        private readonly byte[] _multipleElemBytes = {
                                                         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC2, 0xC2,
                                                         0x04, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0x71, 0x77, 0x28, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x01, 0x00, 0x00, 0x00, 0x34, 0x34, 0x14, 0x00, 0x01, 0x00,
                                                         0x00, 0x00,
                                                         0x01, 0x00, 0x00, 0x00, 0xD4, 0xAB, 0xAB, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC2, 0xC2,
                                                         0x04, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x40, 0x34,
                                                         0x14, 0x00,
                                                         0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0xD7, 0xEB,
                                                         0x9F, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x23, 0x24, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00,
                                                         0x00, 0x00,
                                                         0x4C, 0x34, 0x14, 0x00, 0x14, 0x00, 0x00, 0x00, 0x14, 0x00,
                                                         0x00, 0x00,
                                                         0x14, 0x30, 0x63, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0x4B, 0x5B, 0x66, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0x58, 0x34, 0x14, 0x00, 0x0A, 0x00,
                                                         0x00, 0x00,
                                                         0x0A, 0x00, 0x00, 0x00, 0xAF, 0x1E, 0x33, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF5, 0xFC,
                                                         0x31, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4C, 0x34,
                                                         0x14, 0x00,
                                                         0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x9F, 0xD0,
                                                         0x03, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x61, 0x61, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x58, 0x34, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00,
                                                         0x00, 0x00,
                                                         0xAF, 0xF7, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x00, 0x00, 0x00, 0x00, 0xC2, 0xC2, 0x04, 0x00, 0x00, 0x00,
                                                         0x00, 0x00,
                                                         0x00, 0x00, 0x00, 0x00
                                                     };


        private TracingCallTreeElem _rootElem;
        private MemoryStream _memoryStream;

        [Test]
        public void AllBytesReadFromStreamTest()
        {
            Assert.AreEqual(_memoryStream.Length, _memoryStream.Position);
        }

        [Test]
        public void FieldsDeserializationTest()
        {
            var callTreeElem = new TracingCallTreeElem();
            MemoryStream memoryStream = _singleElemBytes.ConvertToMemoryStream();
            callTreeElem.Deserialize(memoryStream);

            Assert.AreEqual(ExpectedFunctionId, callTreeElem.FunctionId);
            Assert.AreEqual(ExpectedEnterCount, callTreeElem.EnterCount);
            Assert.AreEqual(ExpectedLeaveCount, callTreeElem.LeaveCount);
            Assert.AreEqual(ExpectedWallClockDurationHns, callTreeElem.WallClockDurationHns);
            Assert.AreEqual(ExpectedKernelModeDurationHns, callTreeElem.KernelModeDurationHns);
            Assert.AreEqual(ExpectedUserModeDurationHns, callTreeElem.UserModeDurationHns);
            Assert.AreEqual(ExpectedChildrenCount, callTreeElem.ChildrenCount);

            Assert.AreEqual(memoryStream.Length, memoryStream.Position);
        }

        [Test]
        public void StructureTest()
        {
            Assert.AreEqual(1, _rootElem.ChildrenCount);
            Assert.AreEqual(_rootElem.ChildrenCount, _rootElem.Children.Length);
            Assert.AreEqual(3, _rootElem.Children[0].ChildrenCount);
            Assert.AreEqual(2, _rootElem.Children[0].Children[0].ChildrenCount);
            Assert.AreEqual(0, _rootElem.Children[0].Children[0].Children[0].ChildrenCount);
            Assert.AreEqual(0, _rootElem.Children[0].Children[0].Children[1].ChildrenCount);
            Assert.AreEqual(0, _rootElem.Children[0].Children[1].ChildrenCount);
            Assert.AreEqual(0, _rootElem.Children[0].Children[2].ChildrenCount);
        }

        [Test]
        public void TreeDeserializationTest()
        {
            //0x143458:[TestAssembly]TestNamespace.TestClass.TestMessageWith2Arguments(testArgumentA, testArgumentB),Twc=331eafs,Tum=31fcf5s,Tkm=0s,Ec=a,Lc=a
            TracingCallTreeElem tracingCallTreeElem = _rootElem.Children[0].Children[0].Children[1];
            Assert.AreEqual(0x143458, tracingCallTreeElem.FunctionId);
            Assert.AreEqual(0x331eaf, tracingCallTreeElem.WallClockDurationHns);
            Assert.AreEqual(0x31fcf5, tracingCallTreeElem.UserModeDurationHns);
            Assert.AreEqual(0, tracingCallTreeElem.KernelModeDurationHns);
            Assert.AreEqual(0xA, tracingCallTreeElem.EnterCount);
            Assert.AreEqual(0xA, tracingCallTreeElem.LeaveCount);
        }
    }
}
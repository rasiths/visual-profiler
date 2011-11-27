﻿using System;
using System.IO;
using NUnit.Framework;
using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerAccessTests.MetadataTests;

namespace VisualProfilerAccessTests.ProfilingDataTests.CallTreesTests
{
    [TestFixture]
    public class TracingCallTreeTest
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _twoTreesStream = _twoTreesBytes.ConvertToMemoryStream();
            _callTree1 = TracingCallTree.DeserializeCallTree(_twoTreesStream);
            _callTree2 = TracingCallTree.DeserializeCallTree(_twoTreesStream);
        }

        #endregion

        private readonly byte[] _singleTreeBytes = {0x34, 0x00, 0x00, 0x00, 0x18, 0xE3, 0x4D, 0x00, 0x00};

        private readonly byte[] _twoTreesBytes = {
                                                     0x34, 0x00, 0x00, 0x00, 0x18, 0xE3, 0x49, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x00, 0x00, 0x61, 0x61, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10,
                                                     0x16, 0x26,
                                                     0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x34, 0x34,
                                                     0x2C, 0x00,
                                                     0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x59, 0xAE, 0x97,
                                                     0x00, 0x00,
                                                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0xC2, 0xC2,
                                                     0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x40,
                                                     0x34, 0x2C,
                                                     0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x2B, 0x79,
                                                     0x8B, 0x00,
                                                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x84,
                                                     0x85, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                                                     0x4C, 0x34,
                                                     0x2C, 0x00, 0x14, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x84,
                                                     0xCB, 0x5A,
                                                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x05, 0x13, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x58,
                                                     0x34, 0x2C, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00,
                                                     0x48, 0xBE,
                                                     0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x00, 0xD2, 0xD8, 0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x4C, 0x34, 0x2C, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
                                                     0x00, 0xAF,
                                                     0xF7, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x00, 0x00, 0xC2, 0xC2, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x00, 0x58, 0x34, 0x2C, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00,
                                                     0x00, 0x00,
                                                     0xAF, 0xF7, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x00, 0x00, 0x00, 0x61, 0x61, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x00, 0x00, 0x34, 0x00, 0x00, 0x00, 0xB8, 0x89, 0x4A, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                     0x00, 0x00,
                                                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                                                 };


        private UInt32 ExpectedThreadId = 0x4de318;
        private ProfilingDataTypes ExpectedProfilingDataType = (ProfilingDataTypes) 0x34;
        private MemoryStream _twoTreesStream;
        private TracingCallTree _callTree1;
        private TracingCallTree _callTree2;

        [Test]
        public void AllBytesReadFromStreamTest()
        {
            Assert.AreEqual(_twoTreesStream.Length, _twoTreesStream.Position);
        }

        [Test]
        public void DeserializationWithoutCallTreeElemsTest()
        {
            TracingCallTree callTree = TracingCallTree.DeserializeCallTree(_singleTreeBytes.ConvertToMemoryStream(),
                                                                           false);
            Assert.AreEqual(ExpectedThreadId, callTree.ThreadId);
            Assert.AreEqual(ExpectedProfilingDataType, ProfilingDataTypes.Tracing);
            Assert.AreEqual(ExpectedProfilingDataType, callTree.ProfilingDataType);
        }

        [Test]
        public void TreeInitializationTest()
        {
            Assert.IsNotNull(_callTree1);
            Assert.IsNotNull(_callTree1.RootElem);
            Assert.AreEqual(2, _callTree1.RootElem.Children[0].Children[0].ChildrenCount);
            Assert.AreEqual(0, _callTree1.RootElem.Children[0].Children[0].Children[0].ChildrenCount);

            Assert.IsNotNull(_callTree2);
            Assert.IsNotNull(_callTree2.RootElem);
            Assert.AreEqual(0, _callTree2.RootElem.ChildrenCount);
        }
    }
}
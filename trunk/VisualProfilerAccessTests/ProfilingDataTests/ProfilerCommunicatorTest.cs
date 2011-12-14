using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTreeElems;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerAccess;


namespace VisualProfilerAccessTests.ProfilingDataTests
{
    [TestFixture]
    public class ProfilerCommunicatorTest
    {
        private ProfilerCommunicator<StubCallTree> _profilerCommunicator;
        private ProfilingDataUpdateEventArgs<StubCallTree> _profilingDataUpdateEventArgs;
        readonly AutoResetEvent _updateCallbackFinishedSync = new AutoResetEvent(false);

        [TestFixtureSetUp]
        public void SetUpAttribute()
        {
            var mockMethodCache = new Mock<MetadataCache<MethodMetadata>>();
            var mockMetadataDeserializer = new Mock<MetadataDeserializer>(MockBehavior.Default, null, null, null, null, null);
            mockMetadataDeserializer.Setup(mmd => mmd.DeserializeAllMetadataAndCacheIt(It.IsAny<Stream>()));

            Mock<Stream> mockStream = new Mock<Stream>();

            var mockTreeElemFactory = new Mock<ICallTreeElemFactory<StubCallTreeElem>>();
            var mockTreeElem = new Mock<StubCallTreeElem>(MockBehavior.Default, mockStream.Object, mockTreeElemFactory.Object, mockMethodCache.Object);
            mockTreeElemFactory.Setup(tef => tef.GetCallTreeElem(It.IsAny<Stream>(), It.IsAny<MetadataCache<MethodMetadata>>())).Returns(mockTreeElem.Object);


            var mockTree = new Mock<StubCallTree>(MockBehavior.Default, mockStream.Object, mockTreeElemFactory.Object, mockMethodCache.Object);
            var mockTreeFactory = new Mock<ICallTreeFactory<StubCallTree>>();
            mockTreeFactory.Setup(mtf => mtf.GetCallTree(It.IsAny<Stream>(), It.IsAny<MetadataCache<MethodMetadata>>())).
                Returns(mockTree.Object).Callback<Stream, MetadataCache<MethodMetadata>>(
                (st, met) =>
                {
                    const int stubTreeByteCount = 1;
                    var bytes = new byte[stubTreeByteCount];
                    st.Read(bytes, 0, stubTreeByteCount);
                });

            _profilerCommunicator = new ProfilerCommunicator<StubCallTree>(
                ProfilerTypes.TracingProfiler,
                mockTreeFactory.Object,
                mockMetadataDeserializer.Object,
                mockMethodCache.Object,
                UpdateCallback);
        }

        [SetUp]
        public void TestSetUp()
        {
            _profilingDataUpdateEventArgs = null;
            _updateCallbackFinishedSync.Reset();
        }

        [Test]
        public void SendingProfilingDataActionTest()
        {
            MemoryStream memoryStream = new MemoryStream();

            var actionBytes = BitConverter.GetBytes((int)Actions.SendingProfilingData);
            memoryStream.Write(actionBytes, 0, actionBytes.Length);

            const uint numberOfCallTreesInStream = 4;
            const uint streamLength = numberOfCallTreesInStream;

            var streamLengthBytes = BitConverter.GetBytes(streamLength);
            memoryStream.Write(streamLengthBytes, 0, streamLengthBytes.Length);

            var metadataLenthBytes = BitConverter.GetBytes((uint)0);
            memoryStream.Write(metadataLenthBytes, 0, metadataLenthBytes.Length);
            memoryStream.Position = 0;

            bool finishProfiling;
            _profilerCommunicator.ReceiveActionFromProfilee(memoryStream, out finishProfiling);

            Assert.IsFalse(finishProfiling);
            Assert.AreEqual(memoryStream.Length, memoryStream.Position);

            _updateCallbackFinishedSync.WaitOne();
            Assert.IsNotNull(_profilingDataUpdateEventArgs);
            Assert.AreEqual(numberOfCallTreesInStream, _profilingDataUpdateEventArgs.CallTrees.Count());
            Assert.AreEqual(ProfilerTypes.TracingProfiler, _profilingDataUpdateEventArgs.ProfilerType);
            Assert.AreEqual(Actions.SendingProfilingData, _profilingDataUpdateEventArgs.Action);
        }

        private void UpdateCallback(object sender, ProfilingDataUpdateEventArgs<StubCallTree> profilingDataUpdateEventArgs)
        {
            _profilingDataUpdateEventArgs = profilingDataUpdateEventArgs;
            _updateCallbackFinishedSync.Set();
        }

        [Test]
        public void ProfilingFinishedActionTest()
        {
            MemoryStream memoryStream = new MemoryStream();

            var actionBytes = BitConverter.GetBytes((int)Actions.ProfilingFinished);
            memoryStream.Write(actionBytes, 0, actionBytes.Length);
            memoryStream.Position = 0;

            bool finishProfiling;
            _profilerCommunicator.ReceiveActionFromProfilee(memoryStream, out finishProfiling);

            Assert.IsTrue(finishProfiling);
            Assert.AreEqual(memoryStream.Length, memoryStream.Position);
            Assert.IsNull(_profilingDataUpdateEventArgs);
        }

        [Test]
        public void ErrorActionTest()
        {
            MemoryStream memoryStream = new MemoryStream();

            var actionBytes = BitConverter.GetBytes((int)Actions.Error);
            memoryStream.Write(actionBytes, 0, actionBytes.Length);
            memoryStream.Position = 0;

            bool finishProfiling;
            _profilerCommunicator.ReceiveActionFromProfilee(memoryStream, out finishProfiling);

            Assert.IsTrue(finishProfiling);
            Assert.AreEqual(memoryStream.Length, memoryStream.Position);
            Assert.IsNull(_profilingDataUpdateEventArgs);
        }

        [Test]
        public void SendCommandToProfileeTest()
        {
            MemoryStream stream = new MemoryStream();
            _profilerCommunicator.SendCommandToProfilee(stream, Commands.SendProfilingData);
            Assert.AreEqual(4,stream.Position);
            stream.Position = 0;
            Commands sentCommands = (Commands)stream.DeserializeUint32();
            Assert.AreEqual(Commands.SendProfilingData, sentCommands);
        }
    }
}

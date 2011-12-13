using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData.CallTrees;

namespace VisualProfilerAccess.ProfilingData
{
    public class ProfilerCommunicator<TCallTree> where TCallTree : CallTree
    {
        private readonly ICallTreeFactory<TCallTree> _callTreeFactory;
        private readonly MetadataDeserializer _metadataDeserializer;
        private readonly MetadataCache<MethodMetadata> _methodCache;
        private readonly EventHandler<ProfilingDataUpdateEventArgs<TCallTree>> _updateCallback;
        private readonly ProfilerTypes _profilerType;

        public ProfilerCommunicator(
            ProfilerTypes profilerType,
            ICallTreeFactory<TCallTree> callTreeFactory,
            MetadataDeserializer metadataDeserializer,
            MetadataCache<MethodMetadata> methodCache,
            EventHandler<ProfilingDataUpdateEventArgs<TCallTree>> updateCallback)
        {
            Contract.Requires(callTreeFactory != null);
            Contract.Requires(updateCallback != null);

            _callTreeFactory = callTreeFactory;
            _metadataDeserializer = metadataDeserializer;
            _methodCache = methodCache;
            _updateCallback = updateCallback;
            _profilerType = profilerType;
        }

        public void ReceiveActionFromProfilee(Stream byteStream, out bool finishProfiling)
        {
            Actions receivedAction = byteStream.DeserializeActions();
            var eventArgs = new ProfilingDataUpdateEventArgs<TCallTree>
                                {
                                    Action = receivedAction,
                                    ProfilerType = _profilerType,
                                    ProfilingDataType = ProfilingDataTypes.Nothing
                                };

            switch (receivedAction)
            {
                case Actions.SendingProfilingData:
                    var streamLengthBytes = new byte[sizeof(UInt32)];
                    byteStream.Read(streamLengthBytes, 0, streamLengthBytes.Length);

                    uint streamLength = BitConverter.ToUInt32(streamLengthBytes, 0);
                    var profilingDataBytes = new byte[streamLength];
                    byteStream.Read(profilingDataBytes, 0, profilingDataBytes.Length);

                    var profilingDataStream = new MemoryStream(profilingDataBytes);

                    _metadataDeserializer.DeserializeAllMetadataAndCacheIt(profilingDataStream);

                    var callTrees = new List<TCallTree>();
                    while (profilingDataStream.Position < profilingDataStream.Length)
                    {
                        var callTree = _callTreeFactory.GetCallTree(profilingDataStream, _methodCache);
                        callTrees.Add(callTree);
                    }
                    eventArgs.CallTrees = callTrees;
                    break;

                case Actions.ProfilingFinished:
                    eventArgs.CallTrees = null;
                    finishProfiling = true;
                    return;

                default:
                    eventArgs.Action = Actions.Error;
                    eventArgs.CallTrees = null;
                    finishProfiling = true;
                    return;
                    
            }
            ThreadPool.QueueUserWorkItem(notUsed => _updateCallback(this, eventArgs));
            finishProfiling = false;
        }

        public void SendCommandToProfilee(Stream byteStream)
        {
            byte[] commandBytes = BitConverter.GetBytes((UInt32)Commands.SendProfilingData);
            byteStream.Write(commandBytes, 0, commandBytes.Length);

        }
    }
}
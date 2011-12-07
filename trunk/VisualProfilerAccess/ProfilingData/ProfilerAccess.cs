using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData.CallTrees;

namespace VisualProfilerAccess.ProfilingData
{
    public class ProfilerAccess<TCallTree> where TCallTree : CallTree, new()
    {
        private const string NamePipeName = "VisualProfilerAccessPipe";
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task _actionReceiverTask;
        private Task _commandSenderTask;
        private NamedPipeServerStream _pipeServer;

        public ProfilerAccess(ProcessStartInfo profileeProcessStartInfo, ProfilerTypes profilerType,
                              TimeSpan profilingDataUpdatePeriod,
                              EventHandler<ProfilingDataUpdateEventArgs<TCallTree>> updateCallback)
        {
            Contract.Requires(profileeProcessStartInfo != null);
            Contract.Requires(updateCallback != null);
            ProfileeProcessStartInfo = profileeProcessStartInfo;
            ProfilerType = profilerType;
            ProfilerDataUpdatePeriod = profilingDataUpdatePeriod;
            UpdateCallback = updateCallback;
        }

        private Guid ProfilerCClassGuid
        {
            get
            {
                string profilerGuidString = string.Format("{{19840906-C001-0000-000C-00000000000{0}}}",
                                                          (int)ProfilerType);
                var profilerGuid = new Guid(profilerGuidString);
                return profilerGuid;
            }
        }

        public ProcessStartInfo ProfileeProcessStartInfo { get; private set; }
        public ProfilerTypes ProfilerType { get; private set; }
        public TimeSpan ProfilerDataUpdatePeriod { get; set; }

        public Process ProfileeProcess { get; set; }
        public EventHandler<ProfilingDataUpdateEventArgs<TCallTree>> UpdateCallback { get; private set; }

        private void InitNamePipe()
        {
            _pipeServer = new NamedPipeServerStream("VisualProfilerAccessPipe", PipeDirection.InOut, 1,
                                                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        private void StartReceiveActionsFromProfilee()
        {
            _actionReceiverTask = new Task(ReceiveActionsFromProfilee, _cancellationTokenSource,
                                           TaskCreationOptions.LongRunning);
            _actionReceiverTask.Start();
        }

        private void StartSendingCommandsToProfilee()
        {
            _commandSenderTask = new Task(SendCommandsToProfilee, _cancellationTokenSource.Token,
                                          TaskCreationOptions.LongRunning);
            _commandSenderTask.Start();
        }

        private void StartProfileeProcess()
        {
            ProfileeProcessStartInfo.EnvironmentVariables.Add("COR_ENABLE_PROFILING", "1");
            ProfileeProcessStartInfo.EnvironmentVariables.Add("COR_PROFILER", ProfilerCClassGuid.ToString("B"));
            ProfileeProcessStartInfo.EnvironmentVariables.Add("COR_PROFILER_PATH", @"D:\Honzik\Desktop\visual-profiler\Debug\VisualProfilerBackend.dll");
            ProfileeProcessStartInfo.EnvironmentVariables.Add("VisualProfiler.PipeName", NamePipeName);
            ProfileeProcessStartInfo.UseShellExecute = false;
            ProfileeProcess = Process.Start(ProfileeProcessStartInfo);
        }

        private void ReceiveActionsFromProfilee(object state)
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            while (!cancellationToken.IsCancellationRequested)
            {
                Actions receivedAction = _pipeServer.DeserializeActions();
                var eventArgs = new ProfilingDataUpdateEventArgs<TCallTree>();
                eventArgs.Action = receivedAction;
                eventArgs.ProfilerAccess = this;
                eventArgs.ProfilerType = ProfilerType;
                eventArgs.ProfilingDataType = ProfilingDataTypes.Nothing;

                switch (receivedAction)
                {
                    case Actions.SendingProfilingData:
                        var streamLengthBytes = new byte[sizeof(UInt32)];
                        _pipeServer.Read(streamLengthBytes, 0, streamLengthBytes.Length);

                        uint streamLength = BitConverter.ToUInt32(streamLengthBytes, 0);
                        var profilingDataBytes = new byte[streamLength];
                        _pipeServer.Read(profilingDataBytes, 0, profilingDataBytes.Length);

                        var profilingDataStream = new MemoryStream(profilingDataBytes);
                        MetadataDeserializer.DeserializeAllMetadataAndCacheIt(profilingDataStream);

                        var callTrees = new List<TCallTree>();
                        while (profilingDataStream.Position < profilingDataStream.Length)
                        {
                            var callTree = new TCallTree();
                            callTree.Deserialize(profilingDataStream);
                            callTrees.Add(callTree);
                        }
                        eventArgs.CallTrees = callTrees;
                        break;

                    case Actions.ProfilingFinished:
                        eventArgs.CallTrees = null;
                        _cancellationTokenSource.Cancel();
                        return;

                    default:
                        eventArgs.Action = Actions.Error;
                        eventArgs.CallTrees = null;
                        _cancellationTokenSource.Cancel();
                        return;
                }
                ThreadPool.QueueUserWorkItem(notUsed => UpdateCallback(this, eventArgs));
            }
        }

        private void SendCommandsToProfilee(object state)
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            _pipeServer.WaitForConnection();
            StartReceiveActionsFromProfilee();
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] commandBytes = BitConverter.GetBytes((UInt32)Commands.SendProfilingData);

                try
                {
                    _pipeServer.Write(commandBytes, 0, commandBytes.Length);
                }
                catch (IOException)
                {
                    bool problemOccurredBeforeCancalation = !cancellationToken.IsCancellationRequested;
                    if (problemOccurredBeforeCancalation) throw;
                }

                Thread.Sleep(ProfilerDataUpdatePeriod);
            }
        }

        public void StartProfiler()
        {
            InitNamePipe();
            StartSendingCommandsToProfilee();
            StartProfileeProcess();
        }

        public void Wait()
        {
            _commandSenderTask.Wait();
            _actionReceiverTask.Wait();
        }
    }
}
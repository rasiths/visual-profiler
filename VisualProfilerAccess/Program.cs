using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTrees;

namespace VisualProfilerAccess
{
    public class ProfilerAccess<TCallTree> where TCallTree : CallTree, new()
    {
        private const string NamePipeName = "VisualProfilerAccessPipe";
        private Task _actionReceiverTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task _commandSenderTask;
        private NamedPipeServerStream _pipeServer;

        private Guid ProfilerCClassGuid
        {
            get
            {
                string profilerGuidString = string.Format("{{19840906-C001-0000-000C-00000000000{0}}}", (int)ProfilerType);
                Guid profilerGuid = new Guid(profilerGuidString);
                return profilerGuid;
            }
        }

        public ProcessStartInfo ProfileeProcessStartInfo { get; private set; }
        public ProfilerTypes ProfilerType { get; private set; }
        public TimeSpan ProfilerDataUpdatePeriod { get; set; }

        public Process ProfileeProcess { get; set; }
        public EventHandler<ProfilerDataUpdateEventArgs<TCallTree>> UpdateCallback { get; private set; }

        public ProfilerAccess(ProcessStartInfo profileeProcessStartInfo, ProfilerTypes profilerType, TimeSpan profilerDataUpdatePeriod, EventHandler<ProfilerDataUpdateEventArgs<TCallTree>> updateCallback)
        {
            Contract.Requires(profileeProcessStartInfo != null);
            Contract.Requires(updateCallback != null);
            ProfileeProcessStartInfo = profileeProcessStartInfo;
            ProfilerType = profilerType;
            ProfilerDataUpdatePeriod = profilerDataUpdatePeriod;
            UpdateCallback = updateCallback;
        }

        private void InitNamePipe()
        {
            _pipeServer = new NamedPipeServerStream("VisualProfilerAccessPipe", PipeDirection.InOut, 1,
                                                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        private void StartReceiveActionsFromProfilee()
        {
            _actionReceiverTask = new Task(ReceiveActionsFromProfilee, _cancellationTokenSource, TaskCreationOptions.LongRunning);
            _actionReceiverTask.Start();
        }

        private void StartSendingCommandsToProfilee()
        {
            _commandSenderTask = new Task(SendCommandsToProfilee, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _commandSenderTask.Start();
        }

        private void StartProfileeProcess()
        {
            ProfileeProcessStartInfo.EnvironmentVariables.Add("COR_ENABLE_PROFILING", "1");
            ProfileeProcessStartInfo.EnvironmentVariables.Add("COR_PROFILER", ProfilerCClassGuid.ToString("B"));
            ProfileeProcessStartInfo.EnvironmentVariables.Add("COR_PROFILER_PATH",
                                                              @"D:\Honzik\Desktop\visual-profiler\Debug\VisualProfilerBackend.dll");
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
                var eventArgs = new ProfilerDataUpdateEventArgs<TCallTree>();
                eventArgs.Action = receivedAction;
                eventArgs.ProfilerAccess = this;
                eventArgs.ProfilerType = ProfilerType;
                eventArgs.ProfilingDataType = ProfilingDataTypes.Nothing;

                switch (receivedAction)
                {
                    case Actions.SendingProfilingData:
                        byte[] streamLengthBytes = new byte[sizeof(UInt32)];
                        _pipeServer.Read(streamLengthBytes, 0, streamLengthBytes.Length);

                        var streamLength = BitConverter.ToUInt32(streamLengthBytes, 0);
                        byte[] profilingDataBytes = new byte[streamLength];
                        _pipeServer.Read(profilingDataBytes, 0, profilingDataBytes.Length);

                        MemoryStream profilingDataStream = new MemoryStream(profilingDataBytes);
                        MetadataDeserializer.DeserializeAllMetadataAndCacheIt(profilingDataStream);

                        List<TCallTree> callTrees = new List<TCallTree>();
                        while (profilingDataStream.Position < profilingDataStream.Length)
                        {
                            TCallTree callTree = new TCallTree();
                            callTree.Deserialize(profilingDataStream);
                            callTrees.Add(callTree);
                        }
                        eventArgs.CallTrees = callTrees;
                        break;

                    case Actions.ProfilingFinished:
                        _cancellationTokenSource.Cancel();
                        return;

                    default:
                        eventArgs.Action = Actions.Error;
                        eventArgs.CallTrees = null;
                        _cancellationTokenSource.Cancel();
                        return;
                }
                ThreadPool.QueueUserWorkItem(o => UpdateCallback(this, eventArgs));
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

        public void SendCommandsToProfilee(object state)
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            _pipeServer.WaitForConnection();
            StartReceiveActionsFromProfilee();
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] commandBytes = BitConverter.GetBytes((UInt32)Commands.SendProfilingData);
                _pipeServer.Write(commandBytes, 0, commandBytes.Length);
                Thread.Sleep(ProfilerDataUpdatePeriod);
            }
        }
    }

    public class ProfilerDataUpdateEventArgs : EventArgs
    {
        public ProfilerTypes ProfilerType { get; set; }
        public ProfilingDataTypes ProfilingDataType { get; set; }
        public Actions Action { get; set; }
    }


    public class ProfilerDataUpdateEventArgs<TCallTree> : ProfilerDataUpdateEventArgs where TCallTree : CallTree, new()
    {
        public List<TCallTree> CallTrees { get; set; }
        public ProfilerAccess<TCallTree> ProfilerAccess { get; set; }
    }

    public enum ProfilerTypes
    {
        Nothing = 0,
        SamplingProfiler = 1,
        TracingProfiler = 2
    }

    enum Commands
    {
        SendProfilingData = 101
    }

    public enum Actions
    {
        SendingProfilingData = 201,
        ProfilingFinished = 202,
        Error = 203
    }


    class Program
    {
     
        //static void ReadActions(object o)
        //{
        //    while (true)
        //    {
        //        Actions action = _pipeServer.DeserializeActions();
        //        switch (action)
        //        {
        //            case Actions.SendingProfilingData:
        //                byte[] byteSizeBytes = new byte[sizeof(UInt32)];
        //                _pipeServer.Read(byteSizeBytes, 0, byteSizeBytes.Length);
        //                var streamLength = BitConverter.ToUInt32(byteSizeBytes, 0);

        //                //if (streamLength == 0)
        //                //    return;

        //                byte[] bytes = new byte[streamLength];

        //                _pipeServer.Read(bytes, 0, bytes.Length);

        //                MemoryStream memoryStream = new MemoryStream(bytes);
        //                MetadataDeserializer.DeserializeAllMetadataAndCacheIt(memoryStream);


        //                //Console.ForegroundColor = Console.ForegroundColor == ConsoleColor.Blue ? ConsoleColor.Red : ConsoleColor.Blue;
        //                Console.Clear();

        //                //Console.WriteLine("Methods={0}, Classes={1}, Modules={2}, Assemblies={3}, TreeSize={4}KB", 
        //                //    MethodMetadata.Cache.Count,
        //                //    ClassMetadata.Cache.Count,
        //                //    ModuleMetadata.Cache.Count,
        //                //    AssemblyMetadata.Cache.Count,
        //                //    (memoryStream.Length - memoryStream.Position)/(1024.0)
        //                //    );

        //                //foreach (var assembly in AssemblyMetadata.Cache.Values)
        //                //{
        //                //    Console.WriteLine("{0}, mdToken={1}",assembly.MdToken, assembly.Name);
        //                //}

        //                while (memoryStream.Position < memoryStream.Length)
        //                {
        //                    Console.WriteLine("Methods={0}, Classes={1}, ");
        //                    var deserializeCallTree = TracingCallTree.DeserializeCallTree(memoryStream);
        //                    //  var deserializeCallTree = SamplingCallTree.DeserializeCallTree(memoryStream);
        //                    var s = deserializeCallTree.ToString();
        //                    Console.WriteLine(s);
        //                    Console.WriteLine();
        //                }
        //                break;
        //            case Actions.ProfilingFinished:
        //                _stop = true;
        //                return;

        //            default:
        //                //exception was thrown in the profilee...
        //                goto case Actions.ProfilingFinished;
        //        }
        //    }
        //}


        static void Main(string[] args)
        {

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-us");
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"D:\Honzik\Desktop\Mandelbrot\Mandelbrot\bin\Debug\Mandelbrot.exe";

            var profilerAccess = new ProfilerAccess<TracingCallTree>(processStartInfo,
                                                                     ProfilerTypes.TracingProfiler,
                                                                     TimeSpan.FromMilliseconds(500),
                                                                     UpdateCallback);
            profilerAccess.StartProfiler();
            profilerAccess.Wait();
            Console.WriteLine("bye bye");
        }

        private static void UpdateCallback(object sender, ProfilerDataUpdateEventArgs<TracingCallTree> eventArgs)
        {
            Console.Clear();
            foreach (var callTree in eventArgs.CallTrees)
            {
                string callTreeString = callTree.ToString();
                Console.WriteLine(callTree);
                Console.WriteLine();
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Microsoft.Cci;
using Ninject;
using Ninject.Modules;
using Ninject.Parameters;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTreeElems;
using VisualProfilerAccess.ProfilingData.CallTrees;
using VisualProfilerAccess.SourceLocation;

namespace VisualProfilerAccess
{
    class Program2
    {
        static void Main2(string[] args)
        {



            PdbReader pdbReader = new PdbReader(@"D:\Honzik\Desktop\Mandelbrot\Mandelbrot\bin\Debug\Mandelbrot.exe");
            IModule module = pdbReader.Module;

            foreach (var namedTypeDefinition in module.GetAllTypes())
            {
                PropertyInfo propertyInfo2 = namedTypeDefinition.GetType().GetProperty("TokenValue", BindingFlags.NonPublic | BindingFlags.Instance);
                uint value2 = (uint)propertyInfo2.GetValue(namedTypeDefinition, null);

                foreach (var methodDefinition in namedTypeDefinition.Methods)
                {
                    Console.WriteLine(methodDefinition.Name);

                    PropertyInfo propertyInfo = methodDefinition.GetType().GetProperty("TokenValue", BindingFlags.NonPublic | BindingFlags.Instance);
                    uint value = (uint)propertyInfo.GetValue(methodDefinition, null);
                    foreach (var location in methodDefinition.Locations)
                    {
                        foreach (var primarySourceLocation in pdbReader.GetAllPrimarySourceLocationsFor(location))
                        {

                            if (primarySourceLocation != null)
                            {
                                Console.WriteLine("line {0}, {1}:{2}", primarySourceLocation.StartLine,
                                                  primarySourceLocation.StartColumn, primarySourceLocation.EndColumn);

                            }
                        }
                    }
                }
            }
        }
     }


  

    public class VisualProfilerModule : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<ISourceLocatorFactory>().To<SourceLocatorFactory>().InSingletonScope();
            Kernel.Bind<MetadataCache<MethodMetadata>>().ToSelf().InSingletonScope();
            Kernel.Bind<MetadataCache<ClassMetadata>>().ToSelf().InSingletonScope();
            Kernel.Bind<MetadataCache<ModuleMetadata>>().ToSelf().InSingletonScope();
            Kernel.Bind<MetadataCache<AssemblyMetadata>>().ToSelf().InSingletonScope();
            TracingBindings();
            SamplingBindings();
        }

        private void TracingBindings()
        {
            Kernel.Bind<ProfilerTypes>().ToConstant(ProfilerTypes.TracingProfiler).WhenInjectedInto
                <ProfilerCommunicator<TracingCallTree>>();

            Kernel.Bind<ICallTreeFactory<TracingCallTree>>().To<TracingCallTreeFactory>().InSingletonScope();
        }

        private void SamplingBindings()
        {
            Kernel.Bind<ProfilerTypes>().ToConstant(ProfilerTypes.SamplingProfiler).WhenInjectedInto
                <ProfilerCommunicator<SamplingCallTree>>();

            Kernel.Bind<ICallTreeFactory<SamplingCallTree>>().To<SamplingCallTreeFactory>().InSingletonScope();
        }
    }

    

    internal class Program
    {
        private static void Main(string[] args)
        {
            StandardKernel kernel = new StandardKernel(new VisualProfilerModule());
            

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-us");
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"D:\Honzik\Desktop\Mandelbrot\Mandelbrot\bin\Debug\Mandelbrot.exe";

            if (false)
            {
                EventHandler<ProfilingDataUpdateEventArgs<TracingCallTree>> updateCallback = OnUpdateCallback;
                var updateCallbackParam = new ConstructorArgument("updateCallback", updateCallback);
               
                var profilerAccess = new ProfilerAccess<TracingCallTree>(
                    processStartInfo,
                    ProfilerTypes.TracingProfiler,
                    TimeSpan.FromMilliseconds(1000),
                    kernel.Get<ProfilerCommunicator<TracingCallTree>>(updateCallbackParam));

                profilerAccess.StartProfiler();
                profilerAccess.Wait();
                Console.WriteLine("bye bye");
            }
            else
            {
                EventHandler<ProfilingDataUpdateEventArgs<SamplingCallTree>> updateCallback2 = OnUpdateCallback2;
                var updateCallbackParam = new ConstructorArgument("updateCallback", updateCallback2);
                var profilerAccess = new ProfilerAccess<SamplingCallTree>(
                    processStartInfo,
                    ProfilerTypes.SamplingProfiler,
                    TimeSpan.FromMilliseconds(1000),
                    kernel.Get<ProfilerCommunicator<SamplingCallTree>>(updateCallbackParam));

                profilerAccess.StartProfiler();
                profilerAccess.Wait();
                Console.WriteLine("bye bye");
            }
        }

        private static void OnUpdateCallback(object sender, ProfilingDataUpdateEventArgs<TracingCallTree> eventArgs)
        {
            Console.Clear();
            foreach (var callTree in
                eventArgs.CallTrees)
            {
                ulong totalCycleTime = callTree.TotalCycleTime();
                ulong totalUserKernelTime = callTree.TotalUserKernelTime();
                string callTreeString = callTree.ToString((sb, cte) =>
                                                              {
                                                                  decimal cycleTime = totalCycleTime == 0
                                                                                          ? 0
                                                                                          : cte.CycleTime *
                                                                                            (decimal)
                                                                                            totalUserKernelTime /
                                                                                            totalCycleTime / 1e7M;

                                                                  sb.AppendFormat(",Tact={0:0.00000}s", cycleTime);
                                                              });
                Console.WriteLine(callTreeString);
                Console.WriteLine();

            }
        }

        private static void OnUpdateCallback2(object sender, ProfilingDataUpdateEventArgs<SamplingCallTree> eventArgs)
        {
            Console.Clear();
            foreach (var callTree in
                eventArgs.CallTrees)
            {


                Console.WriteLine(callTree.ToString(null));
                Console.WriteLine();

            }
        }


    }
}
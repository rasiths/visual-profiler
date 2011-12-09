using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Microsoft.Cci;
using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTreeElems;
using VisualProfilerAccess.ProfilingData.CallTrees;

namespace VisualProfilerAccess
{
    class Program2
    {
        static void Main(string[] args)
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

    internal class Program
    {


        private static void Main2(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-us");
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"D:\Honzik\Desktop\Mandelbrot\Mandelbrot\bin\Debug\Mandelbrot.exe";
            
            if (true)
            {

                var profilerAccess = new ProfilerAccess<TracingCallTree>(processStartInfo,
                                                                         ProfilerTypes.TracingProfiler,
                                                                         TimeSpan.FromMilliseconds(1000),
                                                                         OnUpdateCallback);

                profilerAccess.StartProfiler();
                profilerAccess.Wait();
                Console.WriteLine("bye bye");
            }
            else
            {
                var profilerAccess = new ProfilerAccess<SamplingCallTree>(processStartInfo,
                                                                        ProfilerTypes.SamplingProfiler,
                                                                        TimeSpan.FromMilliseconds(1000),
                                                                        OnUpdateCallback2);

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
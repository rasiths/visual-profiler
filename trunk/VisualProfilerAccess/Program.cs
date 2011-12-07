using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using VisualProfilerAccess.ProfilingData;
using VisualProfilerAccess.ProfilingData.CallTreeElems;
using VisualProfilerAccess.ProfilingData.CallTrees;

namespace VisualProfilerAccess
{
    internal class Program
    {
        private static void Main(string[] args)
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
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using VisualProfilerAccess.ProfilingData;
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

            var profilerAccess = new ProfilerAccess<SamplingCallTree>(processStartInfo,
                                                                      ProfilerTypes.SamplingProfiler,
                                                                      TimeSpan.FromMilliseconds(500),
                                                                      (sender, eventArgs) =>
                                                                          {
                                                                              Console.Clear();
                                                                              foreach (
                                                                                  SamplingCallTree callTree in
                                                                                      eventArgs.CallTrees)
                                                                              {
                                                                                  string callTreeString =
                                                                                      callTree.ToString();
                                                                                  Console.WriteLine(callTree);
                                                                                  Console.WriteLine();
                                                                              }
                                                                          });
            profilerAccess.StartProfiler();
            profilerAccess.Wait();
            Console.WriteLine("bye bye");
        }
    }
}
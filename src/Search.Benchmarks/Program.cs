using BenchmarkDotNet.Running;
using System.Reflection;

namespace SCGraphTheory.Search.Benchmarks
{
    internal static class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            // See https://benchmarkdotnet.org/articles/guides/console-args.html (or run app with --help)
            BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
        }
    }
}

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
            // Also see debug launch profiles for some specific command lines (obv run them in "Release"
            // and without a debugger attached).
            BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
        }
    }
}

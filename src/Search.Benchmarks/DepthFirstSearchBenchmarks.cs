#pragma warning disable SA1600 // Elements should be documented
using BenchmarkDotNet.Attributes;
using SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Search;
using SCGraphTheory.Search.Classic;
using System;
using AltValGridGraph = SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Graphs.ValGridGraph<float>;
using RefGridGraph = SCGraphTheory.Search.TestGraphs.ALGridGraph<float>;
using ValGridGraph = SCGraphTheory.Search.TestGraphs.ValGridGraph<float>;

namespace SCGraphTheory.Search.Benchmarks
{
    [MemoryDiagnoser]
    [InProcess]
    public class DepthFirstSearchBenchmarks
    {
        [Benchmark]
        [BenchmarkCategory(nameof(RefGridGraph))]
        public void RefGridGraph() => new DepthFirstSearch<RefGridGraph.Node, RefGridGraph.Edge>(
            source: BenchmarkGraphs.RefGridGraph[0, 0],
            isTarget: BenchmarkGraphs.RefGridGraphIsFarCorner).Complete();

        [Benchmark]
        [BenchmarkCategory(nameof(ValGridGraph))]
        public void ValGridGraph() => new DepthFirstSearch<ValGridGraph.Node, ValGridGraph.Edge>(
            source: BenchmarkGraphs.ValGridGraph[0, 0],
            isTarget: BenchmarkGraphs.ValGridGraphIsFarCorner).Complete();

        [Benchmark]
        [BenchmarkCategory(nameof(AltValGridGraph))]
        public void AltValGridGraph() => new DepthFirstSearch<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>(
            source: BenchmarkGraphs.AltValGridGraph[0, 0],
            isTarget: BenchmarkGraphs.AltValGridGraphIsFarCorner).Complete();

        private static float EuclideanDistance((int x, int y) a, (int x, int y) b)
        {
            return (float)Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
        }
    }
}
#pragma warning restore SA1600
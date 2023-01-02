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
    // NB: unfair tests - largely constant costs means Dijkstra's pretty much same as BFS with more work..
    [MemoryDiagnoser]
    [InProcess]
    public class DijkstraSearchBenchmarks
    {
        [Benchmark]
        [BenchmarkCategory("Dijkstra", nameof(ValGridGraph))]
        public void ValDijkstra() => new DijkstraSearch<ValGridGraph.Node, ValGridGraph.Edge>(
            source: BenchmarkGraphs.ValGridGraph[0, 0],
            isTarget: BenchmarkGraphs.ValGridGraphIsFarCorner,
            getEdgeCost: e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates)).Complete();

        [Benchmark]
        [BenchmarkCategory("Dijkstra", nameof(AltValGridGraph))]
        public void AltValDijkstra() => new AlternativeAbstractions.TEdges.Search.DijkstraSearch<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>(
            source: BenchmarkGraphs.AltValGridGraph[0, 0],
            isTarget: BenchmarkGraphs.AltValGridGraphIsFarCorner,
            getEdgeCost: e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates)).Complete();

        [Benchmark]
        [BenchmarkCategory("Dijkstra", nameof(RefGridGraph))]
        public void RefDijkstra() => new DijkstraSearch<RefGridGraph.Node, RefGridGraph.Edge>(
            source: BenchmarkGraphs.RefGridGraph[0, 0],
            isTarget: BenchmarkGraphs.RefGridGraphIsFarCorner,
            getEdgeCost: e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates)).Complete();

        private static float EuclideanDistance((int x, int y) a, (int x, int y) b)
        {
            return (float)Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
        }
    }
}
#pragma warning restore SA1600
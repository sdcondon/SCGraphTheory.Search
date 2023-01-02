#pragma warning disable SA1600 // Elements should be documented
using BenchmarkDotNet.Attributes;
using SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Search;
using SCGraphTheory.Search.Classic;
using AltValGridGraph = SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Graphs.ValGridGraph<float>;
using RefGridGraph = SCGraphTheory.Search.TestGraphs.ALGridGraph<float>;
using ValGridGraph = SCGraphTheory.Search.TestGraphs.ValGridGraph<float>;

namespace SCGraphTheory.Search.Benchmarks
{
    [MemoryDiagnoser]
    [InProcess]
    public class BreadthFirstSearchBenchmarks
    {
        [Benchmark]
        [BenchmarkCategory(nameof(ValGridGraph))]
        public void ValGridGraph() => new BreadthFirstSearch<ValGridGraph.Node, ValGridGraph.Edge>(
            source: BenchmarkGraphs.ValGridGraph[0, 0],
            isTarget: BenchmarkGraphs.ValGridGraphIsFarCorner).Complete();

        [Benchmark]
        [BenchmarkCategory(nameof(AltValGridGraph))]
        public void AltValGridGraph() => new BreadthFirstSearch<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>(
            source: BenchmarkGraphs.AltValGridGraph[0, 0],
            isTarget: BenchmarkGraphs.AltValGridGraphIsFarCorner).Complete();

        [Benchmark]
        [BenchmarkCategory(nameof(RefGridGraph))]
        public void RefGridGraph() => new BreadthFirstSearch<RefGridGraph.Node, RefGridGraph.Edge>(
            source: BenchmarkGraphs.RefGridGraph[0, 0],
            isTarget: BenchmarkGraphs.RefGridGraphIsFarCorner).Complete();
    }
}
#pragma warning restore SA1600
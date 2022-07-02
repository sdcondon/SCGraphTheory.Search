#pragma warning disable SA1600 // Elements should be documented
using BenchmarkDotNet.Attributes;
using SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Search;
using SCGraphTheory.Search.Classic;
using System;
using AltValGridGraph = SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Graphs.ValGridGraph<float>;
using RefGridGraph = SCGraphTheory.Search.TestGraphs.GridGraph<float>;
using ValGridGraph = SCGraphTheory.Search.TestGraphs.ValGridGraph<float>;

namespace SCGraphTheory.Search.Benchmarks
{
    [MemoryDiagnoser]
    [InProcess]
    public class ClassicSearchBenchmarks
    {
        private const int SIZE = 20;

        private readonly AltValGridGraph altValGraph;
        private readonly ValGridGraph valGraph;
        private readonly RefGridGraph refGraph;

        public ClassicSearchBenchmarks()
        {
            altValGraph = new AltValGridGraph((SIZE, SIZE));
            valGraph = new ValGridGraph((SIZE, SIZE));
            refGraph = new RefGridGraph((SIZE, SIZE), (_, _) => true);
        }

        [Benchmark]
        [BenchmarkCategory("BFS", nameof(ValGridGraph))]
        public void ValBFS() => new BreadthFirstSearch<ValGridGraph.Node, ValGridGraph.Edge>(
            source: valGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        [BenchmarkCategory("DFS", nameof(ValGridGraph))]
        public void ValDFS() => new DepthFirstSearch<ValGridGraph.Node, ValGridGraph.Edge>(
            source: valGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        // NB: unfair test - largely constant costs means Dijkstra's pretty much same as BFS with more work..
        [Benchmark]
        [BenchmarkCategory("Dijkstra", nameof(ValGridGraph))]
        public void ValDijkstra() => new DijkstraSearch<ValGridGraph.Node, ValGridGraph.Edge>(
            source: valGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1),
            getEdgeCost: e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates)).Complete();

        [Benchmark]
        [BenchmarkCategory("A*", nameof(ValGridGraph))]
        public void ValAStar() => new AStarSearch<ValGridGraph.Node, ValGridGraph.Edge>(
            source: valGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1),
            getEdgeCost: e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates),
            getEstimatedCostToTarget: n => EuclideanDistance((SIZE - 1, SIZE - 1), n.Coordinates)).Complete();

        [Benchmark]
        [BenchmarkCategory("BFS", nameof(AltValGridGraph))]
        public void AltValBFS() => new BreadthFirstSearch<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>(
            source: altValGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        [BenchmarkCategory("DFS", nameof(AltValGridGraph))]
        public void AltValDFS() => new DepthFirstSearch<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>(
            source: altValGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        [BenchmarkCategory("Dijkstra", nameof(AltValGridGraph))]
        public void AltValDijkstra() => new DijkstraSearch<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>(
            source: altValGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1),
            getEdgeCost: e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates)).Complete();

        [Benchmark]
        [BenchmarkCategory("A*", nameof(AltValGridGraph))]
        public void AltValAStar() => new AStarSearch<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>(
            source: altValGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1),
            getEdgeCost: e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates),
            getEstimatedCostToTarget: n => EuclideanDistance((SIZE - 1, SIZE - 1), n.Coordinates)).Complete();

        [Benchmark]
        [BenchmarkCategory("BFS", nameof(RefGridGraph))]
        public void RefBFS() => new BreadthFirstSearch<RefGridGraph.Node, RefGridGraph.Edge>(
            source: refGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        [BenchmarkCategory("DFS", nameof(RefGridGraph))]
        public void RefDFS() => new DepthFirstSearch<RefGridGraph.Node, RefGridGraph.Edge>(
            source: refGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        // NB: unfair test - largely constant costs means Dijkstra's pretty much same as BFS with more work..
        [Benchmark]
        [BenchmarkCategory("Dijkstra", nameof(RefGridGraph))]
        public void RefDijkstra() => new DijkstraSearch<RefGridGraph.Node, RefGridGraph.Edge>(
            source: refGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1),
            getEdgeCost: e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates)).Complete();

        [Benchmark]
        [BenchmarkCategory("A*", nameof(RefGridGraph))]
        public void RefAStar() => new AStarSearch<RefGridGraph.Node, RefGridGraph.Edge>(
            source: refGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1),
            getEdgeCost: e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates),
            getEstimatedCostToTarget: n => EuclideanDistance((SIZE - 1, SIZE - 1), n.Coordinates)).Complete();

        private static float EuclideanDistance((int x, int y) a, (int x, int y) b)
        {
            return (float)Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
        }
    }
}
#pragma warning restore SA1600
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SCGraphTheory.Search.Classic;
using System;
using RefGridGraph = SCGraphTheory.Search.TestGraphs.GridGraph<float>;
using ValGridGraph = SCGraphTheory.Search.TestGraphs.ValGridGraph<float>;

namespace SCGraphTheory.Search.Benchmarks
{
    [MemoryDiagnoser]
    [InProcess]
    public class SearchBenchmarks
    {
        private const int SIZE = 20;

        private readonly ValGridGraph valGraph;
        private readonly RefGridGraph refGraph;

        public SearchBenchmarks()
        {
            valGraph = new ValGridGraph((SIZE, SIZE));
            refGraph = new RefGridGraph((SIZE, SIZE), (_, _) => true);
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        public static void Main()
        {
            BenchmarkRunner.Run<SearchBenchmarks>();
        }

        [Benchmark]
        [BenchmarkCategory("Constructors", nameof(ValGridGraph))]
        public ValGridGraph MakeValGraph() => new ValGridGraph((SIZE, SIZE));

        [Benchmark]
        [BenchmarkCategory("Constructors", nameof(RefGridGraph))]
        public RefGridGraph MakeRefGraph() => new RefGridGraph((SIZE, SIZE), (_, _) => true);

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
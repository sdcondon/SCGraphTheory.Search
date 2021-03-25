using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SCGraphTheory.AdjacencyList;
using SCGraphTheory.Search.Classic;
using Search.Benchmarks.GraphImplementations;
using System;

namespace Search.Benchmarks
{
    [MemoryDiagnoser]
    public class SearchBenchmarks
    {
        private const int SIZE = 20;

        private readonly ValSquareGridGraph<bool> valGraph;
        private readonly Graph<RefSquareGridGraph.Node, RefSquareGridGraph.Edge> refGraph;
        private readonly RefSquareGridGraph.Node originNode;

        public SearchBenchmarks()
        {
            valGraph = new ValSquareGridGraph<bool>((SIZE, SIZE));
            refGraph = RefSquareGridGraph.Create((SIZE, SIZE), out originNode);
        }

        [Benchmark]
        public ValSquareGridGraph<bool> MakeValGraph() => new ValSquareGridGraph<bool>((SIZE, SIZE));

        [Benchmark]
        public Graph<RefSquareGridGraph.Node, RefSquareGridGraph.Edge> MakeRefGraph() => RefSquareGridGraph.Create((SIZE, SIZE), out _);

        [Benchmark]
        public void ValBFS() => new BreadthFirstSearch<ValSquareGridGraph<bool>.Node, ValSquareGridGraph<bool>.Edge>(
            valGraph[0, 0],
            n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        public void ValDFS() => new DepthFirstSearch<ValSquareGridGraph<bool>.Node, ValSquareGridGraph<bool>.Edge>(
            valGraph[0, 0],
            n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        public void ValDijkstra() => new DijkstraSearch<ValSquareGridGraph<bool>.Node, ValSquareGridGraph<bool>.Edge>(
            valGraph[0, 0],
            n => n.Coordinates == (SIZE - 1, SIZE - 1),
            e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates)).Complete(); // NB: unfair test - largely constant costs means Dijkstra's pretty much same as BFS with more work..

        [Benchmark]
        public void ValAStar() => new AStarSearch<ValSquareGridGraph<bool>.Node, ValSquareGridGraph<bool>.Edge>(
            valGraph[0, 0],
            n => n.Coordinates == (SIZE - 1, SIZE - 1),
            e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates),
            n => EuclideanDistance((SIZE - 1, SIZE - 1), n.Coordinates)).Complete();

        [Benchmark]
        public void RefBFS() => new BreadthFirstSearch<RefSquareGridGraph.Node, RefSquareGridGraph.Edge>(
            originNode,
            n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        public void RefDFS() => new DepthFirstSearch<RefSquareGridGraph.Node, RefSquareGridGraph.Edge>(
            originNode,
            n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        public void RefDijkstra() => new DijkstraSearch<RefSquareGridGraph.Node, RefSquareGridGraph.Edge>(
            originNode,
            n => n.Coordinates == (SIZE - 1, SIZE - 1),
            e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates)).Complete(); // NB: unfair test - largely constant costs means Dijkstra's pretty much same as BFS with more work..

        [Benchmark]
        public void RefAStar()
        {
            new AStarSearch<RefSquareGridGraph.Node, RefSquareGridGraph.Edge>(
                originNode,
                n => n.Coordinates == (SIZE - 1, SIZE - 1),
                e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates),
                n => EuclideanDistance((SIZE - 1, SIZE - 1), n.Coordinates)).Complete();
        }

        private static float EuclideanDistance((int x, int y) a, (int x, int y) b)
        {
            return (float)Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<SearchBenchmarks>();
        }
    }
}
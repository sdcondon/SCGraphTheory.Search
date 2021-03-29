using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SCGraphTheory.AdjacencyList;
using SCGraphTheory.Search.Benchmarks.GraphImplementations;
using SCGraphTheory.Search.Classic;
using System;

namespace SCGraphTheory.Search.Benchmarks
{
    [MemoryDiagnoser]
    public class SearchBenchmarks
    {
        private const int SIZE = 20;

        private readonly ValGridGraph<bool> valGraph;
        private readonly Graph<RefGridGraph.Node, RefGridGraph.Edge> refGraph;
        private readonly RefGridGraph.Node originNode;

        public SearchBenchmarks()
        {
            valGraph = new ValGridGraph<bool>((SIZE, SIZE));
            refGraph = RefGridGraph.Create((SIZE, SIZE), out originNode);
        }

        [Benchmark]
        public ValGridGraph<bool> MakeValGraph() => new ValGridGraph<bool>((SIZE, SIZE));

        [Benchmark]
        public Graph<RefGridGraph.Node, RefGridGraph.Edge> MakeRefGraph() => RefGridGraph.Create((SIZE, SIZE), out _);

        [Benchmark]
        public void ValBFS() => new BreadthFirstSearch<ValGridGraph<bool>.Node, ValGridGraph<bool>.Edge>(
            valGraph[0, 0],
            n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        public void ValDFS() => new DepthFirstSearch<ValGridGraph<bool>.Node, ValGridGraph<bool>.Edge>(
            valGraph[0, 0],
            n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        public void ValDijkstra() => new DijkstraSearch<ValGridGraph<bool>.Node, ValGridGraph<bool>.Edge>(
            valGraph[0, 0],
            n => n.Coordinates == (SIZE - 1, SIZE - 1),
            e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates)).Complete(); // NB: unfair test - largely constant costs means Dijkstra's pretty much same as BFS with more work..

        [Benchmark]
        public void ValAStar() => new AStarSearch<ValGridGraph<bool>.Node, ValGridGraph<bool>.Edge>(
            valGraph[0, 0],
            n => n.Coordinates == (SIZE - 1, SIZE - 1),
            e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates),
            n => EuclideanDistance((SIZE - 1, SIZE - 1), n.Coordinates)).Complete();

        [Benchmark]
        public void RefBFS() => new BreadthFirstSearch<RefGridGraph.Node, RefGridGraph.Edge>(
            originNode,
            n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        public void RefDFS() => new DepthFirstSearch<RefGridGraph.Node, RefGridGraph.Edge>(
            originNode,
            n => n.Coordinates == (SIZE - 1, SIZE - 1)).Complete();

        [Benchmark]
        public void RefDijkstra() => new DijkstraSearch<RefGridGraph.Node, RefGridGraph.Edge>(
            originNode,
            n => n.Coordinates == (SIZE - 1, SIZE - 1),
            e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates)).Complete(); // NB: unfair test - largely constant costs means Dijkstra's pretty much same as BFS with more work..

        [Benchmark]
        public void RefAStar()
        {
            new AStarSearch<RefGridGraph.Node, RefGridGraph.Edge>(
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
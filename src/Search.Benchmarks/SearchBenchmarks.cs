using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SCGraphTheory.Search.Classic;
using System;
using System.Reflection;
using AltValGridGraph = SCGraphTheory.Search.Benchmarks.AlternativeImplementations.IAltGraph.ValGridGraph<float>;
using RefGridGraph = SCGraphTheory.Search.TestGraphs.GridGraph<float>;
using ValGridGraph = SCGraphTheory.Search.TestGraphs.ValGridGraph<float>;

namespace SCGraphTheory.Search.Benchmarks
{
    [MemoryDiagnoser]
    [InProcess]
    public class SearchBenchmarks
    {
        private const int SIZE = 20;

        private readonly AltValGridGraph altValGraph;
        private readonly ValGridGraph valGraph;
        private readonly RefGridGraph refGraph;

        public SearchBenchmarks()
        {
            altValGraph = new AltValGridGraph((SIZE, SIZE));
            valGraph = new ValGridGraph((SIZE, SIZE));
            refGraph = new RefGridGraph((SIZE, SIZE), (_, _) => true);
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            // See https://benchmarkdotnet.org/articles/guides/console-args.html (or run app with --help)
            BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
        }

        [Benchmark]
        [BenchmarkCategory("Constructors", nameof(ValGridGraph))]
        public ValGridGraph MakeValGraph() => new ValGridGraph((SIZE, SIZE));

        [Benchmark]
        [BenchmarkCategory("Constructors", nameof(RefGridGraph))]
        public RefGridGraph MakeRefGraph() => new RefGridGraph((SIZE, SIZE), (_, _) => true);

        [Benchmark]
        [BenchmarkCategory("EdgeEnumeration", nameof(ValGridGraph))]
        public int ValGraphEdgeEnumerator()
        {
            var node = (INode<ValGridGraph.Node, ValGridGraph.Edge>)valGraph[0, 0];
            int i = 0;
            foreach (var edge in node.Edges)
            {
                i++;
            }

            return i;
        }

        [Benchmark]
        [BenchmarkCategory("EdgeEnumeration", nameof(AltValGridGraph))]
        public int AltValGraphEdgeEnumerator()
        {
            var node = (AlternativeImplementations.IAltGraph.INode<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>)altValGraph[0, 0];
            int i = 0;
            foreach (var edge in node.Edges)
            {
                i++;
            }

            return i;
        }

        [Benchmark]
        [BenchmarkCategory("BFS", nameof(AltValGridGraph))]
        public void AltValBFS()
        {
            var search = new AlternativeImplementations.IAltGraph.BreadthFirstSearch<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>(
            source: altValGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1));
            while (!search.IsConcluded)
            {
                search.NextStep();
            }
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
        [BenchmarkCategory("A*", nameof(AltValGridGraph))]
        public void AltValAStar()
        {
            var search = new AlternativeImplementations.IAltGraph.AStarSearch<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>(
            source: altValGraph[0, 0],
            isTarget: n => n.Coordinates == (SIZE - 1, SIZE - 1),
            getEdgeCost: e => EuclideanDistance(e.To.Coordinates, e.From.Coordinates),
            getEstimatedCostToTarget: n => EuclideanDistance((SIZE - 1, SIZE - 1), n.Coordinates));
            while (!search.IsConcluded)
            {
                search.NextStep();
            }
        }

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
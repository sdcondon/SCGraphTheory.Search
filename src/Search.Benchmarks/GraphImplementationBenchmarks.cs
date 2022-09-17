#pragma warning disable SA1600 // Elements should be documented
using BenchmarkDotNet.Attributes;
using SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges;
using AltValGridGraph = SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Graphs.ValGridGraph<float>;
using RefGridGraph = SCGraphTheory.Search.TestGraphs.ALGridGraph<float>;
using ValGridGraph = SCGraphTheory.Search.TestGraphs.ValGridGraph<float>;

namespace SCGraphTheory.Search.Benchmarks
{
    [MemoryDiagnoser]
    [InProcess]
    public class GraphImplementationBenchmarks
    {
        private const int SIZE = 20;

        private readonly AltValGridGraph altValGraph;
        private readonly ValGridGraph valGraph;
        private readonly RefGridGraph refGraph;

        public GraphImplementationBenchmarks()
        {
            altValGraph = new AltValGridGraph((SIZE, SIZE));
            valGraph = new ValGridGraph((SIZE, SIZE));
            refGraph = new RefGridGraph((SIZE, SIZE), (_, _) => true);
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
            var node = (INode<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>)altValGraph[0, 0];
            int i = 0;
            foreach (var edge in node.Edges)
            {
                i++;
            }

            return i;
        }
    }
}
#pragma warning restore SA1600

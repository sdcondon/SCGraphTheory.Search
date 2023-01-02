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
        [Benchmark]
        [BenchmarkCategory("Constructors", nameof(ValGridGraph))]
        public ValGridGraph MakeValGraph() => new ValGridGraph((20, 20));

        [Benchmark]
        [BenchmarkCategory("Constructors", nameof(RefGridGraph))]
        public RefGridGraph MakeRefGraph() => new RefGridGraph((20, 20), (_, _) => true);

        [Benchmark]
        [BenchmarkCategory("EdgeEnumeration", nameof(ValGridGraph))]
        public int ValGraphEdgeEnumerator()
        {
            var node = (INode<ValGridGraph.Node, ValGridGraph.Edge>)BenchmarkGraphs.ValGridGraph[0, 0];
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
            var node = (INode<AltValGridGraph.Node, AltValGridGraph.Edge, AltValGridGraph.EdgeCollection>)BenchmarkGraphs.AltValGridGraph[0, 0];
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

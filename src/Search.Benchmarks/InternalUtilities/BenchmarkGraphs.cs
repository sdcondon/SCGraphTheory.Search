using SCGraphTheory.Search.TestGraphs;
using System;
using AltValGridGraph = SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Graphs.ValGridGraph<float>;
using RefGridGraph = SCGraphTheory.Search.TestGraphs.ALGridGraph<float>;
using ValGridGraph = SCGraphTheory.Search.TestGraphs.ValGridGraph<float>;

namespace SCGraphTheory.Search.Benchmarks
{
    /// <summary>
    /// Various static graph instances for use by benchmarks.
    /// </summary>
    internal class BenchmarkGraphs
    {
        /// <summary>
        /// Size constant used by the graphs in this constant.
        /// </summary>
        public const int SIZE = 20;

        /// <summary>
        /// Gets a (<see cref="SIZE"/> by <see cref="SIZE"/>) instance of <see cref="ALGridGraph{T}"/>.
        /// </summary>
        public static RefGridGraph RefGridGraph { get; } = new RefGridGraph((SIZE, SIZE), (_, _) => true);

        /// <summary>
        /// Gets a predicate that indicates whether a node in <see cref="RefGridGraph"/> is the node with the maximal co-ordinates.
        /// </summary>
        public static Predicate<RefGridGraph.Node> RefGridGraphIsFarCorner { get; } = n => n.Coordinates == (SIZE - 1, SIZE - 1);

        /// <summary>
        /// Gets a (<see cref="SIZE"/> by <see cref="SIZE"/>) instance of <see cref="ValGridGraph{T}"/>.
        /// </summary>
        public static ValGridGraph ValGridGraph { get; } = new ValGridGraph((SIZE, SIZE));

        /// <summary>
        /// Gets a predicate that indicates whether a node in <see cref="ValGridGraph"/> is the node with the maximal co-ordinates.
        /// </summary>
        public static Predicate<ValGridGraph.Node> ValGridGraphIsFarCorner { get; } = n => n.Coordinates == (SIZE - 1, SIZE - 1);

        /// <summary>
        /// Gets a (<see cref="SIZE"/> by <see cref="SIZE"/>) instance of <see cref="AlternativeAbstractions.TEdges.Graphs.ValGridGraph{T}"/>
        /// (navigating this should result in less boxed enumerators when compared to ValGridGraph).
        /// </summary>
        public static AltValGridGraph AltValGridGraph { get; } = new AltValGridGraph((SIZE, SIZE));

        /// <summary>
        /// Gets a predicate that indicates whether a node in <see cref="AltValGridGraph"/> is the node with the maximal co-ordinates.
        /// </summary>
        public static Predicate<AltValGridGraph.Node> AltValGridGraphIsFarCorner { get; } = n => n.Coordinates == (SIZE - 1, SIZE - 1);
    }
}

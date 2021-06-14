using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.AlternativeImplementations.IAltGraph
{
    /// <summary>
    /// Interface for types representing a graph.
    /// </summary>
    /// <typeparam name="TNode">The type of each node of the graph.</typeparam>
    /// <typeparam name="TEdge">The type of each edge of the graph.</typeparam>
    public interface IGraph<TNode, TEdge, TEdges>
        where TNode : INode<TNode, TEdge, TEdges>
        where TEdge : IEdge<TNode, TEdge, TEdges>
        where TEdges : IReadOnlyCollection<TEdge>
    {
        /// <summary>
        /// Gets the set of nodes of the graph.
        /// </summary>
        IEnumerable<TNode> Nodes { get; }

        /// <summary>
        /// Gets the set of edges of the graph.
        /// </summary>
        IEnumerable<TEdge> Edges { get; }
    }
}

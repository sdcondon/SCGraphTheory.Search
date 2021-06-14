using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.AlternativeImplementations.IAltGraph
{
    /// <summary>
    /// Interface for types representing a node in a <see cref="IGraph{TNode, TEdge}"/>.
    /// </summary>
    /// <typeparam name="TNode">The type of each node of the graph.</typeparam>
    /// <typeparam name="TEdge">The type of each edge of the graph.</typeparam>
    public interface INode<out TNode, out TEdge, out TEdges>
        where TNode : INode<TNode, TEdge, TEdges>
        where TEdge : IEdge<TNode, TEdge, TEdges>
        where TEdges : IReadOnlyCollection<TEdge>
    {
        /// <summary>
        /// Gets the collection of edges that are outbound from this node.
        /// </summary>
        TEdges Edges { get; }
    }
}

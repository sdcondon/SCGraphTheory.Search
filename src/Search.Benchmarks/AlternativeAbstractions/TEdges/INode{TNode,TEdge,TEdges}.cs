using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges
{
    /// <summary>
    /// Interface for types representing a node in a <see cref="IGraph{TNode, TEdge}"/>.
    /// </summary>
    /// <typeparam name="TNode">The type of each node of the graph.</typeparam>
    /// <typeparam name="TEdge">The type of each edge of the graph.</typeparam>
    /// <typeparam name="TEdges">The type of the outbound edges collection of each node of the graph.</typeparam>
    /// <remarks>
    /// This interface exists only to facilitate avoidance of boxing by consumers when <see cref="TEdges"/> is a value type. While the resulting performance boost won't be
    /// massive, it may be desirable in some cases. The vast majority of node implementations can simply implement <see cref="INode{TNode, TEdge}"/> and ignore this.
    /// </remarks>
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

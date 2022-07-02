using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges
{
    /// <summary>
    /// Interface for types representing an edge in a <see cref="IGraph{TNode, TEdge}"/>.
    /// </summary>
    /// <typeparam name="TNode">The type of each node of the graph.</typeparam>
    /// <typeparam name="TEdge">The type of each edge of the graph.</typeparam>
    /// <typeparam name="TEdges">The type of the outbound edges collection of each node of the graph.</typeparam>
    /// <remarks>
    /// This interface exists only to facilitate avoidance of boxing by consumers when <see cref="TEdges"/> is a value type. While the resulting performance boost won't be
    /// massive, it may be desirable in some cases. The vast majority of edge implementations can simply implement <see cref="IEdge{TNode, TEdge}"/> and ignore this.
    /// </remarks>
    public interface IEdge<TNode, TEdge, TEdges>
        where TNode : INode<TNode, TEdge, TEdges>
        where TEdge : IEdge<TNode, TEdge, TEdges>
        where TEdges : IReadOnlyCollection<TEdge>
    {
        /// <summary>
        /// Gets the node that the edge connects from.
        /// </summary>
        TNode From { get; }

        /// <summary>
        /// Gets the node that the edge connects to.
        /// </summary>
        TNode To { get; }
    }
}

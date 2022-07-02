using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges
{
    /// <summary>
    /// Interface for types representing a graph.
    /// </summary>
    /// <typeparam name="TNode">The type of each node of the graph.</typeparam>
    /// <typeparam name="TEdge">The type of each edge of the graph.</typeparam>
    /// <typeparam name="TEdges">The type of the outbound edges collection of each node of the graph.</typeparam>
    /// <remarks>
    /// This interface exists only to facilitate avoidance of boxing by consumers when <see cref="TEdges"/> is a value type. While the resulting performance boost won't be
    /// massive, it may be desirable in some cases. The vast majority of graph implementations can simply implement <see cref="IGraph{TNode, TEdge}"/> and ignore this.
    /// </remarks>
    public interface IGraph<out TNode, out TEdge, out TEdges>
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

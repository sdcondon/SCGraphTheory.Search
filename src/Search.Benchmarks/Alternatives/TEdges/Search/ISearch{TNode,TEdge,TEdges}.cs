using SCGraphTheory.Search.Classic;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.Alternatives.TEdges.Search
{
    /// <summary>
    /// Interface for algorithms that search a graph for a target node.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph being searched.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph being searched.</typeparam>
    /// <typeparam name="TEdges">The type of the node edges collection of the graph being search.</typeparam>
    public interface ISearch<TNode, TEdge, TEdges>
        where TNode : INode<TNode, TEdge, TEdges>
        where TEdge : IEdge<TNode, TEdge, TEdges>
        where TEdges : IReadOnlyCollection<TEdge>
    {
        /// <summary>
        /// Gets a value indicating whether the search is concluded (irrespective of whether a target node was found or not).
        /// </summary>
        bool IsConcluded { get; }

        /// <summary>
        /// Gets the target node if the search is concluded and found a matching node, otherwise returns <see langword="default"/>.
        /// </summary>
        TNode Target { get; }

        /// <summary>
        /// Gets the search tree (or forest). Each visited node is present as a key. The associated value is the edge used to discover it (or <see langword="default"/> for the source node).
        /// </summary>
        IReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>> Visited { get; }

        /// <summary>
        /// Executes the next step of the search.
        /// </summary>
        void NextStep();
    }
}

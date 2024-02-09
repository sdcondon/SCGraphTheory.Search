#if NET6_0_OR_GREATER
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Interface for algorithms that search an async graph for a target node.
    /// </summary>
    /// <typeparam name="TNode">The node type of the async graph being searched.</typeparam>
    /// <typeparam name="TEdge">The edge type of the async graph being searched.</typeparam>
    public interface IAsyncSearch<TNode, TEdge>
        where TNode : IAsyncNode<TNode, TEdge>
        where TEdge : IAsyncEdge<TNode, TEdge>
    {
        /// <summary>
        /// Gets a value indicating whether the search is concluded (irrespective of whether a target node was found or not).
        /// </summary>
        bool IsConcluded { get; }

        /// <summary>
        /// Gets a value indicating whether the search is concluded, and found a target node.
        /// </summary>
        bool IsSucceeded { get; }

        /// <summary>
        /// Gets the target node if the search is concluded and found a matching node, otherwise returns <see langword="default"/>.
        /// </summary>
        TNode Target { get; }

        /// <summary>
        /// Gets the search tree (or forest). Each visited node is present as a key. The associated value is information about the edge used to discover it (or <see langword="default"/> for the source node).
        /// </summary>
        IReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>> Visited { get; }

        /// <summary>
        /// Executes the next step of the search.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token that, if triggered, should cause cancellation of the step. Optional, defaults to <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="ValueTask"/> that returns the edge that was explored by this step.</returns>
        ValueTask<TEdge> NextStepAsync(CancellationToken cancellationToken = default);
    }
}
#endif

#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Implementation of <see cref="IAsyncSearch{TNode, TEdge}"/> that uses the iterative deepening depth-first search algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the async graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the async graph to search.</typeparam>
    public class IterativeDeepeningDepthFirstAsyncSearch<TNode, TEdge> : IAsyncSearch<TNode, TEdge>
        where TNode : IAsyncNode<TNode, TEdge>
        where TEdge : IAsyncEdge<TNode, TEdge>
    {
        private readonly TNode source;
        private readonly Func<TNode, ValueTask<bool>> isTargetAsync;

        private int currentDepthLimit = 0;
        private LimitedDepthFirstAsyncSearch<TNode, TEdge> currentSearch;

        private IterativeDeepeningDepthFirstAsyncSearch(TNode source, Func<TNode, ValueTask<bool>> isTargetAsync)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.source = source;
            this.isTargetAsync = isTargetAsync ?? throw new ArgumentNullException(nameof(isTargetAsync));
            this.currentDepthLimit = 0;
        }

        /// <inheritdoc />
        public bool IsConcluded => currentSearch.IsConcluded && currentSearch.State != LimitedDepthFirstAsyncSearch<TNode, TEdge>.States.CutOff;

        /// <inheritdoc />
        public bool IsSucceeded => currentSearch.IsSucceeded;

        /// <inheritdoc />
        public TNode Target => currentSearch.Target;

        /// <inheritdoc />
        /// <remarks>
        /// NB: This is just the visited dictionary of the current depth-limited search, so periodically collapses back to being empty.
        /// Perhaps not ideal since it temporarily forgets edges and nodes that have been visited. On the other hand, is an accurate representation
        /// of how the search progresses. Leaving like this for now at least.
        /// </remarks>
        public IReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>> Visited => currentSearch.Visited;

        /// <summary>
        /// Creates a new instance of the <see cref="IterativeDeepeningDepthFirstAsyncSearch{TNode, TEdge}"/> class,
        /// and progresses it to the point at which the nodes adjacent to the source node are on the frontier.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask" /> that will return the new search.</returns>
        public static ValueTask<IterativeDeepeningDepthFirstAsyncSearch<TNode, TEdge>> CreateAsync(
            TNode source,
            Predicate<TNode> isTarget,
            CancellationToken cancellationToken = default)
        {
            return CreateAsync(
                source,
                n => ValueTask.FromResult(isTarget(n)),
                cancellationToken);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IterativeDeepeningDepthFirstAsyncSearch{TNode, TEdge}"/> class,
        /// and progresses it to the point at which the nodes adjacent to the source node are on the frontier.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTargetAsync">An async predicate for identifying the target node of the search.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask" /> that will return the new search.</returns>
        public static async ValueTask<IterativeDeepeningDepthFirstAsyncSearch<TNode, TEdge>> CreateAsync(
            TNode source,
            Func<TNode, ValueTask<bool>> isTargetAsync,
            CancellationToken cancellationToken = default)
        {
            var search = new IterativeDeepeningDepthFirstAsyncSearch<TNode, TEdge>(source, isTargetAsync);

            search.currentSearch = await LimitedDepthFirstAsyncSearch<TNode, TEdge>.CreateAsync(source, isTargetAsync, search.currentDepthLimit, cancellationToken);

            return search;
        }

        /// <inheritdoc />
        public async ValueTask<TEdge> NextStepAsync(CancellationToken cancellationToken)
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            if (currentSearch.State == LimitedDepthFirstAsyncSearch<TNode, TEdge>.States.CutOff)
            {
                currentSearch = await LimitedDepthFirstAsyncSearch<TNode, TEdge>.CreateAsync(source, isTargetAsync, ++currentDepthLimit, cancellationToken);
            }

            return await currentSearch.NextStepAsync(cancellationToken);
        }
    }
}
#endif

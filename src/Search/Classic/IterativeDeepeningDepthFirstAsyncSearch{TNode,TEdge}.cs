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
        private readonly Predicate<TNode> isTarget;

        private int currentDepthLimit = 0;
        private LimitedDepthFirstAsyncSearch<TNode, TEdge> currentSearch;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterativeDeepeningDepthFirstAsyncSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public IterativeDeepeningDepthFirstAsyncSearch(TNode source, Predicate<TNode> isTarget)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.source = source;
            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));
            this.currentDepthLimit = 0;
            this.currentSearch = new LimitedDepthFirstAsyncSearch<TNode, TEdge>(source, isTarget, currentDepthLimit);
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

        /// <inheritdoc />
        public async ValueTask<TEdge> NextStepAsync(CancellationToken cancellationToken)
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            if (currentSearch.State == LimitedDepthFirstAsyncSearch<TNode, TEdge>.States.CutOff)
            {
                currentSearch = new LimitedDepthFirstAsyncSearch<TNode, TEdge>(source, isTarget, ++currentDepthLimit);
            }

            return await currentSearch.NextStepAsync(cancellationToken);
        }
    }
}
#endif

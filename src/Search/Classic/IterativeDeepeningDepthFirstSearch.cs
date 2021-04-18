using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses the limited depth-first search algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    public class IterativeDeepeningDepthFirstSearch<TNode, TEdge> : ISearch<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
    {
        private readonly TNode source;
        private readonly Predicate<TNode> isTarget;

        private int currentDepthLimit = 0;
        private LimitedDepthFirstSearch<TNode, TEdge> currentSearch;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterativeDeepeningDepthFirstSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public IterativeDeepeningDepthFirstSearch(TNode source, Predicate<TNode> isTarget)
        {
            this.source = source;
            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));
            this.currentDepthLimit = 0;
            this.currentSearch = new LimitedDepthFirstSearch<TNode, TEdge>(source, isTarget, currentDepthLimit);
        }

        /// <inheritdoc />
        public bool IsConcluded => currentSearch.IsConcluded && currentSearch.State != LimitedDepthFirstSearch<TNode, TEdge>.States.CutOff;

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
        public void NextStep()
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            if (currentSearch.State == LimitedDepthFirstSearch<TNode, TEdge>.States.CutOff)
            {
                currentSearch = new LimitedDepthFirstSearch<TNode, TEdge>(source, isTarget, ++currentDepthLimit);
            }

            currentSearch.NextStep();
        }
    }
}

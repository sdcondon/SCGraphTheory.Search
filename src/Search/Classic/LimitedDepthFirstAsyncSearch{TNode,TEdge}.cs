#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Implementation of <see cref="IAsyncSearch{TNode, TEdge}"/> that uses the limited depth-first search algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the async graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the async graph to search.</typeparam>
    public class LimitedDepthFirstAsyncSearch<TNode, TEdge> : IAsyncSearch<TNode, TEdge>
        where TNode : IAsyncNode<TNode, TEdge>
        where TEdge : IAsyncEdge<TNode, TEdge>
    {
        private readonly Predicate<TNode> isTarget;
        private readonly int depthLimit;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly Stack<FrontierNodeInfo> frontier = new Stack<FrontierNodeInfo>();

        private readonly HashSet<TNode> cutoffNodes = new HashSet<TNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitedDepthFirstAsyncSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="depthLimit">The depth at which the search should be cut off.</param>
        public LimitedDepthFirstAsyncSearch(TNode source, Predicate<TNode> isTarget, int depthLimit)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));
            this.depthLimit = depthLimit;

            Visited = new ReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>>(visited);

            // Initialize the search tree with the source node. NB: unlike the synchronous version,
            // we do NOT immediately visit it. While the caller having to do a NextStepAsync to "discover" it
            // is perhaps unintuitive, queuing up its outbound edges is async here, and we shouldn't be doing
            // potentially long-running operations in a ctor.
            frontier.Push(new (source, default, 0));
            visited[source] = new KnownEdgeInfo<TEdge>(default, true);
        }

        /// <summary>
        /// Enumeration of possible states for <see cref="LimitedDepthFirstSearch{TNode, TEdge}"/>.
        /// </summary>
        public enum States
        {
            /// <summary>
            /// The search is ongoing.
            /// </summary>
            InProgress,

            /// <summary>
            /// The search has concluded without finding a target node because there are no more nodes within the depth limit of the source node.
            /// </summary>
            CutOff,

            /// <summary>
            /// The search has concluded without finding a target node even after exploring all nodes within the graph (assuming a connected graph).
            /// </summary>
            Failed,

            /// <summary>
            /// The search has concluded by finding a target node.
            /// </summary>
            Completed,
        }

        /// <summary>
        /// Gets the current state of the search.
        /// </summary>
        public States State { get; private set; } = States.InProgress;

        /// <inheritdoc />
        public bool IsConcluded => State != States.InProgress;

        /// <inheritdoc />
        public bool IsSucceeded => State == States.Completed;

        /// <inheritdoc />
        public TNode Target { get; private set; }

        /// <inheritdoc />
        public IReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>> Visited { get; }

        /// <inheritdoc />
        public async ValueTask<TEdge> NextStepAsync(CancellationToken cancellationToken)
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var next = frontier.Pop();
            visited[next.node] = new KnownEdgeInfo<TEdge>(next.edgeToNode, false);
            await VisitAsync(next.node, next.depth, cancellationToken);
            return next.edgeToNode;
        }

        private async ValueTask VisitAsync(TNode node, int depth, CancellationToken cancellationToken)
        {
            if (isTarget(node))
            {
                Target = node;
                State = States.Completed;
                return;
            }

            await foreach (var nextEdge in node.Edges.WithCancellation(cancellationToken))
            {
                if (!visited.ContainsKey(nextEdge.To))
                {
                    if (depth < depthLimit)
                    {
                        frontier.Push(new (nextEdge.To, nextEdge, depth + 1));
                        visited[nextEdge.To] = new KnownEdgeInfo<TEdge>(nextEdge, true);
                        cutoffNodes.Remove(nextEdge.To);
                    }
                    else
                    {
                        cutoffNodes.Add(nextEdge.To);
                    }
                }
            }

            if (frontier.Count == 0)
            {
                State = cutoffNodes.Count > 0 ? States.CutOff : States.Failed;
            }
        }

        // TODO: wasteful - only need node or edge. could probably turn me into a union?
        // How would we be able to distinguish, though? An "initialised" field to indicate that the first visit is complete?
        // Hmm. Perhaps should just visit first node in ctor after all..
        private record struct FrontierNodeInfo(TNode node, TEdge edgeToNode, int depth);
    }
}
#endif

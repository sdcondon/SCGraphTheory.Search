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
        private readonly Func<TNode, ValueTask<bool>> isTargetAsync;
        private readonly int depthLimit;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly Stack<FrontierNodeInfo> frontier = new Stack<FrontierNodeInfo>();

        private readonly HashSet<TNode> cutoffNodes = new HashSet<TNode>();

        /// <summary>
        /// Initialises a new instance of the <see cref="LimitedDepthFirstAsyncSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="isTargetAsync">An async predicate for identifying the target node of the search.</param>
        /// <param name="depthLimit">The depth at which the search should be cut off.</param>
        protected LimitedDepthFirstAsyncSearch(Func<TNode, ValueTask<bool>> isTargetAsync, int depthLimit)
        {
            this.isTargetAsync = isTargetAsync ?? throw new ArgumentNullException(nameof(isTargetAsync));
            this.depthLimit = depthLimit;

            Visited = new ReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>>(visited);
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

        /// <summary>
        /// Creates a new instance of the <see cref="LimitedDepthFirstAsyncSearch{TNode, TEdge}"/> class,
        /// and progresses it to the point at which the nodes adjacent to the source node are on the frontier.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="depthLimit">The depth at which the search should be cut off.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask" /> that will return the new search.</returns>
        public static ValueTask<LimitedDepthFirstAsyncSearch<TNode, TEdge>> CreateAsync(
            TNode source,
            Predicate<TNode> isTarget,
            int depthLimit,
            CancellationToken cancellationToken = default)
        {
            return CreateAsync(
                source,
                n => ValueTask.FromResult(isTarget(n)),
                depthLimit,
                cancellationToken);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LimitedDepthFirstAsyncSearch{TNode, TEdge}"/> class,
        /// and progresses it to the point at which the nodes adjacent to the source node are on the frontier.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTargetAsync">An async predicate for identifying the target node of the search.</param>
        /// <param name="depthLimit">The depth at which the search should be cut off.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask" /> that will return the new search.</returns>
        public static async ValueTask<LimitedDepthFirstAsyncSearch<TNode, TEdge>> CreateAsync(
            TNode source,
            Func<TNode, ValueTask<bool>> isTargetAsync,
            int depthLimit,
            CancellationToken cancellationToken = default)
        {
            var search = new LimitedDepthFirstAsyncSearch<TNode, TEdge>(isTargetAsync, depthLimit);
            await search.InitialiseAsync(source, cancellationToken);
            return search;
        }

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

        /// <summary>
        /// Initialises the search by conducting the first search step, which adds all of the 
        /// adjacent nodes of a given source node to the search frontier.
        /// </summary>
        /// <param name="source">The source node of the search.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask"/> representing completion of the operation.</returns>
        protected async ValueTask InitialiseAsync(TNode source, CancellationToken cancellationToken = default)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (visited.Count > 0)
            {
                throw new InvalidOperationException("Search already initialised");
            }

            // Initialize the search tree with the source node and immediately visit it.
            // The caller having to do a NextStep to discover it is unintuitive.
            visited[source] = new KnownEdgeInfo<TEdge>(default, false);
            await VisitAsync(source, 0, cancellationToken);
        }

        private async ValueTask VisitAsync(TNode node, int depth, CancellationToken cancellationToken)
        {
            if (await isTargetAsync(node))
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

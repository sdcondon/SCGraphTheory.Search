﻿#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Implementation of <see cref="IAsyncSearch{TNode, TEdge}"/> that uses the breadth-first search algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the async graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the async graph to search.</typeparam>
    public class BreadthFirstAsyncSearch<TNode, TEdge> : IAsyncSearch<TNode, TEdge>
        where TNode : IAsyncNode<TNode, TEdge>
        where TEdge : IAsyncEdge<TNode, TEdge>
    {
        private readonly Func<TNode, ValueTask<bool>> isTargetAsync;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly Queue<FrontierNodeInfo> frontier = new Queue<FrontierNodeInfo>();

        private BreadthFirstAsyncSearch(Func<TNode, ValueTask<bool>> isTargetAsync)
        {
            this.isTargetAsync = isTargetAsync ?? throw new ArgumentNullException(nameof(isTargetAsync));

            Visited = new ReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>>(visited);
        }

        /// <inheritdoc />
        public bool IsConcluded { get; private set; } = false;

        /// <inheritdoc />
        public bool IsSucceeded { get; private set; } = false;

        /// <inheritdoc />
        public TNode Target { get; private set; } = default;

        /// <inheritdoc />
        public IReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>> Visited { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="BreadthFirstAsyncSearch{TNode, TEdge}"/> class,
        /// and progresses it to the point at which the nodes adjacent to the source node are on the frontier.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask" /> that will return the new search.</returns>
        public static ValueTask<BreadthFirstAsyncSearch<TNode, TEdge>> CreateAsync(
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
        /// Creates a new instance of the <see cref="BreadthFirstAsyncSearch{TNode, TEdge}"/> class,
        /// and progresses it to the point at which the nodes adjacent to the source node are on the frontier.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTargetAsync">An async predicate for identifying the target node of the search.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask" /> that will return the new search.</returns>
        public static async ValueTask<BreadthFirstAsyncSearch<TNode, TEdge>> CreateAsync(
            TNode source,
            Func<TNode, ValueTask<bool>> isTargetAsync,
            CancellationToken cancellationToken = default)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var search = new BreadthFirstAsyncSearch<TNode, TEdge>(isTargetAsync);

            // Initialize the search tree with the source node and immediately visit it.
            // The caller having to do a NextStep to discover it is unintuitive.
            search.visited[source] = new KnownEdgeInfo<TEdge>(default, false);
            await search.VisitAsync(source, cancellationToken);

            return search;
        }

        /// <inheritdoc />
        public async ValueTask<TEdge> NextStepAsync(CancellationToken cancellationToken)
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var (node, edge) = frontier.Dequeue();
            visited[node] = new KnownEdgeInfo<TEdge>(edge, false);
            await VisitAsync(node, cancellationToken);
            return edge;
        }

        private async ValueTask VisitAsync(TNode node, CancellationToken cancellationToken)
        {
            if (await isTargetAsync(node))
            {
                Target = node;
                IsConcluded = true;
                IsSucceeded = true;
                return;
            }

            // TODO-BUG: Iterates whole collection when it shouldn't need to to explore the next edge, and explores
            // edges in reverse order. We should probably be maintaining a queue of enumerators instead.
            await foreach (var nextEdge in node.Edges.WithCancellation(cancellationToken))
            {
                if (!visited.ContainsKey(nextEdge.To))
                {
                    frontier.Enqueue(new (nextEdge.To, nextEdge));
                    visited[nextEdge.To] = new KnownEdgeInfo<TEdge>(nextEdge, true);
                }
            }

            if (frontier.Count == 0)
            {
                IsConcluded = true;
            }
        }

        // TODO: wasteful - only need one or the other. could probably turn me into a union?
        // How would we be able to tell, though? An "initialised" field to indicate that the first visit is complete?
        // Hmm. Perhaps should just visit first node in ctor after all..
        private record struct FrontierNodeInfo(TNode node, TEdge edgeToNode);
    }
}
#endif

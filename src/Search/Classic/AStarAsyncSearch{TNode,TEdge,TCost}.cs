﻿#if NET7_0_OR_GREATER
using SCGraphTheory.Search.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Implementation of <see cref="IAsyncSearch{TNode, TEdge}"/> that uses the A* algorithm and allows for user-specified numeric cost type.
    /// </summary>
    /// <typeparam name="TNode">The node type of the async graph being searched.</typeparam>
    /// <typeparam name="TEdge">The edge type of the async graph being searched.</typeparam>
    /// <typeparam name="TCost">The type of the cost metric.</typeparam>
    public class AStarAsyncSearch<TNode, TEdge, TCost> : IAsyncSearch<TNode, TEdge>
        where TNode : IAsyncNode<TNode, TEdge>
        where TEdge : IAsyncEdge<TNode, TEdge>
        where TCost : INumber<TCost>
    {
        private readonly Predicate<TNode> isTarget;
        private readonly Func<TEdge, TCost> getEdgeCost;
        private readonly Func<TNode, TCost> getEstimatedCostToTarget;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly KeyedPriorityQueue<TNode, FrontierNodeInfo> frontier = new KeyedPriorityQueue<TNode, FrontierNodeInfo>(new FrontierPriorityComparer());

        /// <summary>
        /// Initializes a new instance of the <see cref="AStarAsyncSearch{TNode, TEdge, TCost}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCost">A function for calculating the cost of an edge.</param>
        /// <param name="getEstimatedCostToTarget">A function for estimating the cost to the target from a given node.</param>
        public AStarAsyncSearch(
            TNode source,
            Predicate<TNode> isTarget,
            Func<TEdge, TCost>
            getEdgeCost,
            Func<TNode, TCost> getEstimatedCostToTarget)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));
            this.getEdgeCost = getEdgeCost ?? throw new ArgumentNullException(nameof(getEdgeCost));
            this.getEstimatedCostToTarget = getEstimatedCostToTarget ?? throw new ArgumentNullException(nameof(getEstimatedCostToTarget));

            Visited = new ReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>>(visited);

            // Initialize the search tree with the source node. NB: unlike the synchronous version,
            // we do NOT immediately visit it. While the caller having to do a NextStepAsync to "discover" it
            // is perhaps unintuitive, queuing up its outbound edges is async here, and we shouldn't be doing
            // potentially long-running operations in a ctor.
            frontier.Enqueue(source, new (default, TCost.AdditiveIdentity, getEstimatedCostToTarget(source)));
            visited[source] = new KnownEdgeInfo<TEdge>(default, true);
        }

        /// <inheritdoc />
        public bool IsConcluded { get; private set; } = false;

        /// <inheritdoc />
        public bool IsSucceeded { get; private set; } = false;

        /// <inheritdoc />
        public TNode Target { get; private set; } = default;

        /// <inheritdoc />
        public IReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>> Visited { get; }

        /// <inheritdoc />
        public async ValueTask<TEdge> NextStepAsync(CancellationToken cancellationToken = default)
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var node = frontier.Dequeue(out var frontierInfo);
            visited[node] = new KnownEdgeInfo<TEdge>(frontierInfo.bestEdgeToNode, false);
            await VisitAsync(node, frontierInfo.bestCostToNode, cancellationToken);
            return frontierInfo.bestEdgeToNode;
        }

        private async ValueTask VisitAsync(TNode node, TCost bestCostToNode, CancellationToken cancellationToken)
        {
            if (isTarget(node))
            {
                Target = node;
                IsConcluded = true;
                IsSucceeded = true;
                return;
            }

            await foreach (var edge in node.Edges.WithCancellation(cancellationToken))
            {
                node = edge.To;

                var totalCostToNodeViaEdge = bestCostToNode + getEdgeCost(edge);
                var estimatedTotalCostViaNode = totalCostToNodeViaEdge + getEstimatedCostToTarget(node);

                if (TCost.IsFinite(estimatedTotalCostViaNode))
                {
                    var isAlreadyOnFrontier = frontier.TryGetPriority(node, out var frontierDetails);

                    if (!isAlreadyOnFrontier && !visited.ContainsKey(node))
                    {
                        // Node has not been added to the frontier - add it
                        frontier.Enqueue(node, new (edge, totalCostToNodeViaEdge, estimatedTotalCostViaNode));
                        visited[node] = new KnownEdgeInfo<TEdge>(edge, true);
                    }
                    else if (isAlreadyOnFrontier && totalCostToNodeViaEdge < frontierDetails.bestCostToNode)
                    {
                        // Node is already on the frontier, but the cost via this edge
                        // is cheaper than has been found previously - increase its priority
                        frontier.IncreasePriority(node, new (edge, totalCostToNodeViaEdge, estimatedTotalCostViaNode));
                        visited[node] = new KnownEdgeInfo<TEdge>(edge, true);
                    }
                }
            }

            if (frontier.Count == 0)
            {
                IsConcluded = true;
            }
        }

        private class FrontierPriorityComparer : IComparer<FrontierNodeInfo>
        {
            public int Compare(FrontierNodeInfo x, FrontierNodeInfo y)
            {
                return y.estimatedBestCostViaNode.CompareTo(x.estimatedBestCostViaNode);
            }
        }

        private record struct FrontierNodeInfo(TEdge bestEdgeToNode, TCost bestCostToNode, TCost estimatedBestCostViaNode);
    }
}
#endif
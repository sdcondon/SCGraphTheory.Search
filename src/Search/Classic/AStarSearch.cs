using SCGraphTheory.Search.Utility;
using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses the A* algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph being searched.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph being searched.</typeparam>
    public class AStarSearch<TNode, TEdge> : ISearch<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
    {
        private readonly Predicate<TNode> isTarget;
        private readonly Func<TEdge, float> getEdgeCost;
        private readonly Func<TNode, float> getEstimatedCostToTarget;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly KeyedPriorityQueue<TNode, (TEdge bestEdge, float bestCostToNode, float estimatedBestCostViaNode)> frontier = new KeyedPriorityQueue<TNode, (TEdge, float, float)>(new FrontierPriorityComparer());

        /// <summary>
        /// Initializes a new instance of the <see cref="AStarSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCost">A function for calculating the cost of an edge.</param>
        /// <param name="getEstimatedCostToTarget">A function for estimating the cost to the target from a given node.</param>
        public AStarSearch(
            TNode source,
            Predicate<TNode> isTarget,
            Func<TEdge, float> getEdgeCost,
            Func<TNode, float> getEstimatedCostToTarget)
        {
            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));
            this.getEdgeCost = getEdgeCost ?? throw new ArgumentNullException(nameof(getEdgeCost));
            this.getEstimatedCostToTarget = getEstimatedCostToTarget ?? throw new ArgumentNullException(nameof(getEstimatedCostToTarget));

            // Initialize the frontier with the source node and immediately discover it.
            // The caller having to do a NextStep to discover it is unintuitive.
            UpdateFrontier(source, default, 0);
            NextStep();
        }

        /// <inheritdoc />
        public bool IsConcluded { get; private set; } = false;

        /// <inheritdoc />
        public TNode Target { get; private set; } = default;

        /// <inheritdoc />
        public IReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>> Visited => visited;

        /// <inheritdoc />
        public void NextStep()
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var node = frontier.Dequeue(out var frontierInfo);
            visited[node] = new KnownEdgeInfo<TEdge>(frontierInfo.bestEdge, false);

            if (isTarget(node))
            {
                Target = node;
                IsConcluded = true;
                return;
            }

            foreach (var edge in node.Edges)
            {
                UpdateFrontier(edge.To, edge, frontierInfo.bestCostToNode + getEdgeCost(edge));
            }

            if (frontier.Count == 0)
            {
                IsConcluded = true;
            }
        }

        private void UpdateFrontier(TNode node, TEdge edge, float totalCostToNodeViaEdge)
        {
            var estimatedTotalCostViaNode = totalCostToNodeViaEdge + getEstimatedCostToTarget(node);
            var isAlreadyOnFrontier = frontier.TryGetPriority(node, out var frontierDetails);
            if (!isAlreadyOnFrontier && !visited.ContainsKey(node))
            {
                // Node has not been added to the frontier - add it
                frontier.Enqueue(node, (edge, totalCostToNodeViaEdge, estimatedTotalCostViaNode));
                visited[node] = new KnownEdgeInfo<TEdge>(edge, true);
            }
            else if (isAlreadyOnFrontier && totalCostToNodeViaEdge < frontierDetails.bestCostToNode)
            {
                // Node is already on the frontier, but the cost via this edge
                // is cheaper than has been found previously - increase its priority
                frontier.IncreasePriority(node, (edge, totalCostToNodeViaEdge, estimatedTotalCostViaNode));
                visited[node] = new KnownEdgeInfo<TEdge>(edge, true);
            }
        }

        private class FrontierPriorityComparer : IComparer<(TEdge bestEdge, float bestCostToNode, float estimatedBestCostViaNode)>
        {
            public int Compare((TEdge bestEdge, float bestCostToNode, float estimatedBestCostViaNode) x, (TEdge bestEdge, float bestCostToNode, float estimatedBestCostViaNode) y)
            {
                return y.estimatedBestCostViaNode.CompareTo(x.estimatedBestCostViaNode);
            }
        }
    }
}

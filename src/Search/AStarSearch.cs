using SCGraphTheory.Search.Utility;
using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search
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

        // TODO: Modify priority queue so that it can include the frontier details rather than needing another dictionary (but still allow updating priority by just node).
        private readonly Dictionary<TNode, TEdge> shortestPathTree = new Dictionary<TNode, TEdge>();
        private readonly KeyedPriorityQueue<float, TNode> frontierNodeQueue = new KeyedPriorityQueue<float, TNode>((x, y) => y.CompareTo(x));
        private readonly Dictionary<TNode, (TEdge bestEdge, float bestCost)> frontierDetailsByNode = new Dictionary<TNode, (TEdge, float)>();

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
            this.isTarget = isTarget;
            this.getEdgeCost = getEdgeCost;
            this.getEstimatedCostToTarget = getEstimatedCostToTarget;

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
        public IReadOnlyDictionary<TNode, TEdge> Predecessors => shortestPathTree;

        /// <summary>
        /// Gets the search frontier - the next nodes (and edges leading to them) under consideration in the search.
        /// </summary>
        public IReadOnlyDictionary<TNode, (TEdge bestEdge, float bestCost)> Frontier => frontierDetailsByNode;

        /// <inheritdoc />
        public void NextStep()
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var nextClosestNode = frontierNodeQueue.Dequeue();
            float costToNode;
            (shortestPathTree[nextClosestNode], costToNode) = frontierDetailsByNode[nextClosestNode];
            frontierDetailsByNode.Remove(nextClosestNode);

            if (isTarget(nextClosestNode))
            {
                Target = nextClosestNode;
                IsConcluded = true;
                return;
            }

            foreach (var edge in nextClosestNode.Edges)
            {
                UpdateFrontier(edge.To, edge, costToNode + getEdgeCost(edge));
            }

            if (frontierNodeQueue.Count == 0)
            {
                IsConcluded = true;
            }
        }

        private void UpdateFrontier(TNode node, TEdge edge, float totalCostToNodeViaEdge)
        {
            var estimatedTotalCostViaNode = totalCostToNodeViaEdge + getEstimatedCostToTarget(node);
            var isAlreadyOnFrontier = frontierDetailsByNode.TryGetValue(node, out var frontierDetails);
            if (!isAlreadyOnFrontier && !shortestPathTree.ContainsKey(node))
            {
                // Node has not been added to the frontier - add it, including the total cost to it
                frontierNodeQueue.Enqueue(estimatedTotalCostViaNode, node);
                frontierDetailsByNode[node] = (edge, totalCostToNodeViaEdge);
            }
            else if (isAlreadyOnFrontier && totalCostToNodeViaEdge < frontierDetails.bestCost)
            {
                // Node is already on the frontier, but the cost via this edge
                // is cheaper than has been found previously - update the frontier and costs map
                frontierNodeQueue.IncreasePriority(node, estimatedTotalCostViaNode);
                frontierDetailsByNode[node] = (edge, totalCostToNodeViaEdge);
            }
        }
    }
}

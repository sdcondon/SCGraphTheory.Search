using SCGraphTheory.Search.Utility;
using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search
{
    /// <summary>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses Dijkstra's algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    public class DijkstraSearch<TNode, TEdge> : ISearch<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
    {
        private readonly Predicate<TNode> isTarget;
        private readonly Func<TEdge, float> getEdgeCost;

        // TODO: Modify priority queue so that it can include the frontier details rather than needing another dictionary
        // (but still allow for keying by just node for checking existence and updating priority by just node).
        private readonly Dictionary<TNode, TEdge> shortestPathTree = new Dictionary<TNode, TEdge>();
        private readonly KeyedPriorityQueue<TNode, float> frontierNodeQueue = new KeyedPriorityQueue<TNode, float>((x, y) => y.CompareTo(x));
        private readonly Dictionary<TNode, (TEdge bestEdge, float bestCost)> frontierDetailsByNode = new Dictionary<TNode, (TEdge, float)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DijkstraSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCost">A function for calculating the cost of an edge.</param>
        public DijkstraSearch(TNode source, Predicate<TNode> isTarget, Func<TEdge, float> getEdgeCost)
        {
            this.isTarget = isTarget;
            this.getEdgeCost = getEdgeCost;

            // Initialize the frontier with the source node and immediately discover it.
            // The caller having to do a NextStep to discover it is unintuitive.
            UpdateFrontier(source, default, 0);
            NextStep();
        }

        /// <inheritdoc />
        public bool IsConcluded { get; private set; } = false;

        /// <inheritdoc />
        public TNode Target { get; private set; }

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
            var isAlreadyOnFrontier = frontierDetailsByNode.TryGetValue(node, out var frontierDetails);
            if (!isAlreadyOnFrontier && !shortestPathTree.ContainsKey(node))
            {
                // Node has not been added to the frontier - add it
                frontierNodeQueue.Enqueue(node, totalCostToNodeViaEdge);
                frontierDetailsByNode[node] = (edge, totalCostToNodeViaEdge);
            }
            else if (isAlreadyOnFrontier && totalCostToNodeViaEdge < frontierDetails.bestCost)
            {
                // Node is already on the frontier, but the cost via this edge
                // is cheaper than has been found previously - update the frontier
                frontierNodeQueue.IncreasePriority(node, totalCostToNodeViaEdge);
                frontierDetailsByNode[node] = (edge, totalCostToNodeViaEdge);
            }
        }
    }
}

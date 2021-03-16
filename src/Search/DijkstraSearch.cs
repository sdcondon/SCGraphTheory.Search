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

        private readonly Dictionary<TNode, TEdge> shortestPathTree = new Dictionary<TNode, TEdge>();
        private readonly KeyedPriorityQueue<TNode, (TEdge bestEdge, float bestCost)> frontier = new KeyedPriorityQueue<TNode, (TEdge, float)>(new FrontierPriorityComparer());

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

        /// <inheritdoc />
        public void NextStep()
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var nextClosestNode = frontier.Dequeue(out var frontierInfo);
            shortestPathTree[nextClosestNode] = frontierInfo.bestEdge;

            if (isTarget(nextClosestNode))
            {
                Target = nextClosestNode;
                IsConcluded = true;
                return;
            }

            foreach (var edge in nextClosestNode.Edges)
            {
                UpdateFrontier(edge.To, edge, frontierInfo.bestCost + getEdgeCost(edge));
            }

            if (frontier.Count == 0)
            {
                IsConcluded = true;
            }
        }

        private void UpdateFrontier(TNode node, TEdge edge, float totalCostToNodeViaEdge)
        {
            var isAlreadyOnFrontier = frontier.TryGetPriority(node, out var frontierDetails);
            if (!isAlreadyOnFrontier && !shortestPathTree.ContainsKey(node))
            {
                // Node has not been added to the frontier - add it
                frontier.Enqueue(node, (edge, totalCostToNodeViaEdge));
            }
            else if (isAlreadyOnFrontier && totalCostToNodeViaEdge < frontierDetails.bestCost)
            {
                // Node is already on the frontier, but the cost via this edge
                // is cheaper than has been found previously - update the frontier
                frontier.IncreasePriority(node, (edge, totalCostToNodeViaEdge));
            }
        }

        private class FrontierPriorityComparer : IComparer<(TEdge bestEdge, float bestCost)>
        {
            public int Compare((TEdge bestEdge, float bestCost) x, (TEdge bestEdge, float bestCost) y)
            {
                return y.bestCost.CompareTo(x.bestCost);
            }
        }
    }
}

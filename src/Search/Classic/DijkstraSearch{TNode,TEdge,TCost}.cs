#if NET7_0_OR_GREATER
using SCGraphTheory.Search.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses Dijkstra's algorithm and allows for user-specified numeric cost type.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    /// <typeparam name="TCost">The type of the cost metric.</typeparam>
    public class DijkstraSearch<TNode, TEdge, TCost> : ISearch<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
        where TCost : INumber<TCost>
    {
        private readonly Predicate<TNode> isTarget;
        private readonly Func<TEdge, TCost> getEdgeCost;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly KeyedPriorityQueue<TNode, (TEdge bestEdge, TCost bestCost)> frontier = new KeyedPriorityQueue<TNode, (TEdge, TCost)>(new FrontierPriorityComparer());

        /// <summary>
        /// Initializes a new instance of the <see cref="DijkstraSearch{TNode, TEdge, TCost}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCost">A function for calculating the cost of an edge.</param>
        public DijkstraSearch(TNode source, Predicate<TNode> isTarget, Func<TEdge, TCost> getEdgeCost)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));
            this.getEdgeCost = getEdgeCost ?? throw new ArgumentNullException(nameof(getEdgeCost));

            Visited = new ReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>>(visited);

            // Initialize the search tree with the source node and immediately visit it.
            // The caller having to do a NextStep to discover it is unintuitive.
            visited[source] = new KnownEdgeInfo<TEdge>(default, false);
            Visit(source, TCost.AdditiveIdentity);
        }

        /// <inheritdoc />
        public bool IsConcluded { get; private set; } = false;

        /// <inheritdoc />
        public bool IsSucceeded { get; private set; } = false;

        /// <inheritdoc />
        public TNode Target { get; private set; }

        /// <inheritdoc />
        public IReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>> Visited { get; }

        /// <inheritdoc />
        public TEdge NextStep()
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var node = frontier.Dequeue(out var frontierInfo);
            visited[node] = new KnownEdgeInfo<TEdge>(frontierInfo.bestEdge, false);
            Visit(node, frontierInfo.bestCost);
            return frontierInfo.bestEdge;
        }

        private void Visit(TNode node, TCost bestCost)
        {
            if (isTarget(node))
            {
                Target = node;
                IsConcluded = true;
                IsSucceeded = true;
                return;
            }

            foreach (var edge in node.Edges)
            {
                node = edge.To;

                var totalCostToNodeViaEdge = bestCost + getEdgeCost(edge);
                var isAlreadyOnFrontier = frontier.TryGetPriority(node, out var frontierDetails);

                // NB: we prune infinite costs - making the assumption that the heuristic returning infinity for something means its not interested in pursuing it..
                if (!isAlreadyOnFrontier && !visited.ContainsKey(node) && TCost.IsFinite(totalCostToNodeViaEdge))
                {
                    // Node has not been added to the frontier - add it
                    frontier.Enqueue(node, (edge, totalCostToNodeViaEdge));
                    visited[node] = new KnownEdgeInfo<TEdge>(edge, true);
                }
                else if (isAlreadyOnFrontier && totalCostToNodeViaEdge < frontierDetails.bestCost)
                {
                    // Node is already on the frontier, but the cost via this edge
                    // is cheaper than has been found previously - update the frontier
                    frontier.IncreasePriority(node, (edge, totalCostToNodeViaEdge));
                    visited[node] = new KnownEdgeInfo<TEdge>(edge, true);
                }
            }

            if (frontier.Count == 0)
            {
                IsConcluded = true;
            }
        }

        private class FrontierPriorityComparer : IComparer<(TEdge bestEdge, TCost bestCost)>
        {
            public int Compare((TEdge bestEdge, TCost bestCost) x, (TEdge bestEdge, TCost bestCost) y)
            {
                return y.bestCost.CompareTo(x.bestCost);
            }
        }
    }
}
#endif

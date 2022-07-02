using SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Search.Utility;
using SCGraphTheory.Search.Classic;
using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Search
{
    /// <summary>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses Dijkstra's algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    /// <typeparam name="TEdges">The type of the outbound edges collection of each node of the graph being search.</typeparam>
    public class DijkstraSearch<TNode, TEdge, TEdges> : ISearch<TNode, TEdge, TEdges>
        where TNode : INode<TNode, TEdge, TEdges>
        where TEdge : IEdge<TNode, TEdge, TEdges>
        where TEdges : IReadOnlyCollection<TEdge>
    {
        private readonly Predicate<TNode> isTarget;
        private readonly Func<TEdge, float> getEdgeCost;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly KeyedPriorityQueue<TNode, (TEdge bestEdge, float bestCost)> frontier = new KeyedPriorityQueue<TNode, (TEdge, float)>(new FrontierPriorityComparer());

        /// <summary>
        /// Initializes a new instance of the <see cref="DijkstraSearch{TNode, TEdge, TEdges}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCost">A function for calculating the cost of an edge.</param>
        public DijkstraSearch(TNode source, Predicate<TNode> isTarget, Func<TEdge, float> getEdgeCost)
        {
            // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));
            this.getEdgeCost = getEdgeCost ?? throw new ArgumentNullException(nameof(getEdgeCost));

            // Initialize the search tree with the source node and immediately visit it.
            // The caller having to do a NextStep to discover it is unintuitive.
            visited[source] = new KnownEdgeInfo<TEdge>(default, false);
            Visit(source, 0f);
        }

        /// <inheritdoc />
        public bool IsConcluded { get; private set; } = false;

        /// <inheritdoc />
        public TNode Target { get; private set; }

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
            Visit(node, frontierInfo.bestCost);
        }

        private void Visit(TNode node, float bestCost)
        {
            if (isTarget(node))
            {
                Target = node;
                IsConcluded = true;
                return;
            }

            foreach (var edge in node.Edges)
            {
                node = edge.To;

                var totalCostToNodeViaEdge = bestCost + getEdgeCost(edge);
                var isAlreadyOnFrontier = frontier.TryGetPriority(node, out var frontierDetails);

                if (!isAlreadyOnFrontier && !visited.ContainsKey(node))
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

        private class FrontierPriorityComparer : IComparer<(TEdge bestEdge, float bestCost)>
        {
            public int Compare((TEdge bestEdge, float bestCost) x, (TEdge bestEdge, float bestCost) y)
            {
                return y.bestCost.CompareTo(x.bestCost);
            }
        }
    }
}

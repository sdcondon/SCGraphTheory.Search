#if NET7_0_OR_GREATER
using SCGraphTheory.Search.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

namespace SCGraphTheory.Search.Classic
{
  /// <summary>
  /// <para>
  /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses the A* algorithm and allows for user-specified non-numeric cost type.
  /// Intended as useful if costs are complex in their behaviour - tiered costs, for example, where one component is less important than another,
  /// regardless of how large it gets.
  /// </para>
  /// <para>
  /// NB #1: while the cost does not have to be numeric, there are obviously still a number of constraints on it. Essentially, you
  /// need to be able to add costs together and compare them. You also need an "additive identity" (i.e. a value that makes no change
  /// when added - a "zero").
  /// </para>
  /// <para>
  /// NB #2: One limitation of non-numeric types can arise if you need some notion of "infinite" cost in your search (i.e. some
  /// edge that is considered non-navigable for the purposes of the search). While we can allow for this with numeric costs by checking
  /// for infinite values, MS (very sensibly) hasn't created a separate interface for types with a notion of infinity, so we can't allow
  /// for it here.
  /// </para>
  /// </summary>
  /// <typeparam name="TNode">The node type of the graph being searched.</typeparam>
  /// <typeparam name="TEdge">The edge type of the graph being searched.</typeparam>
  /// <typeparam name="TCost">The type of the cost metric.</typeparam>
  public class AStarSearchWithNonNumericCost<TNode, TEdge, TCost> : ISearch<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
        where TCost : IComparable<TCost>, IComparisonOperators<TCost, TCost, bool>, IAdditionOperators<TCost, TCost, TCost>, IAdditiveIdentity<TCost, TCost>
    {
        private readonly Predicate<TNode> isTarget;
        private readonly Func<TEdge, TCost> getEdgeCost;
        private readonly Func<TNode, TCost> getEstimatedCostToTarget;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly KeyedPriorityQueue<TNode, (TEdge bestEdge, TCost bestCostToNode, TCost estimatedBestCostViaNode)> frontier = new KeyedPriorityQueue<TNode, (TEdge, TCost, TCost)>(new FrontierPriorityComparer());

        /// <summary>
        /// Initializes a new instance of the <see cref="AStarSearchWithNonNumericCost{TNode, TEdge, TCost}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCost">A function for calculating the cost of an edge.</param>
        /// <param name="getEstimatedCostToTarget">A function for estimating the cost to the target from a given node.</param>
        public AStarSearchWithNonNumericCost(
            TNode source,
            Predicate<TNode> isTarget,
            Func<TEdge, TCost> getEdgeCost,
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
        public TNode Target { get; private set; } = default;

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
            Visit(node, frontierInfo.bestCostToNode);
            return frontierInfo.bestEdge;
        }

        private void Visit(TNode node, TCost bestCostToNode)
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

                var totalCostToNodeViaEdge = bestCostToNode + getEdgeCost(edge);
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

            if (frontier.Count == 0)
            {
                IsConcluded = true;
            }
        }

        private class FrontierPriorityComparer : IComparer<(TEdge bestEdge, TCost bestCostToNode, TCost estimatedBestCostViaNode)>
        {
            public int Compare((TEdge bestEdge, TCost bestCostToNode, TCost estimatedBestCostViaNode) x, (TEdge bestEdge, TCost bestCostToNode, TCost estimatedBestCostViaNode) y)
            {
                return y.estimatedBestCostViaNode.CompareTo(x.estimatedBestCostViaNode);
            }
        }
    }
}
#endif
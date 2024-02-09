#if NET7_0_OR_GREATER
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
    /// <para>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses Dijkstra's algorithm and allows for user-specified non-numeric cost type.
    /// Intended as useful if costs are complex in their behaviour - tiered costs, for example, where one component is less important than another,
    /// regardless of how large it gets.
    /// </para>
    /// <para>
    /// NB #1: while the cost does not have to be numeric, there are obviously still a number of constraints on it. Essentially, you
    /// need to be able to add costs together and compare them. You also need an "additive identity" (i.e. a value that makes no change
    /// when added - a "zero").
    /// </para>
    /// <para>
    /// NB #2: the one limitation of using a non-numeric cost type is the inability to account for an "infinite" cost in your search
    /// (i.e. some edge that is considered essentially non-navigable for the purposes of the search). While we can allow for this with numeric
    /// costs by checking for infinite values, MS hasn't created a separate interface for types with a notion of infinity, so we can't allow
    /// for it here.
    /// </para>
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    /// <typeparam name="TCost">The type of the cost metric.</typeparam>
    public class DijkstraAsyncSearchWithNonNumericCost<TNode, TEdge, TCost> : IAsyncSearch<TNode, TEdge>
        where TNode : IAsyncNode<TNode, TEdge>
        where TEdge : IAsyncEdge<TNode, TEdge>
        where TCost : IComparable<TCost>, IComparisonOperators<TCost, TCost, bool>, IAdditionOperators<TCost, TCost, TCost>, IAdditiveIdentity<TCost, TCost>
  {
        private readonly Predicate<TNode> isTarget;
        private readonly Func<TEdge, TCost> getEdgeCost;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly KeyedPriorityQueue<TNode, FrontierNodeInfo> frontier = new KeyedPriorityQueue<TNode, FrontierNodeInfo>(new FrontierPriorityComparer());

        /// <summary>
        /// Initializes a new instance of the <see cref="DijkstraAsyncSearchWithNonNumericCost{TNode, TEdge, TCost}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCost">A function for calculating the cost of an edge.</param>
        public DijkstraAsyncSearchWithNonNumericCost(TNode source, Predicate<TNode> isTarget, Func<TEdge, TCost> getEdgeCost)
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

            // Initialize the search frontier with the source node. NB: unlike the synchronous version,
            // we do NOT immediately visit it. While the caller having to do a NextStepAsync to "discover" it
            // is perhaps unintuitive, queuing up its outbound edges is async here, and we shouldn't be doing
            // potentially long-running operations in a ctor.
            frontier.Enqueue(source, new (default, TCost.AdditiveIdentity));
            visited[source] = new KnownEdgeInfo<TEdge>(default, true);
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
        public async ValueTask<TEdge> NextStepAsync(CancellationToken cancellationToken)
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

        private async ValueTask VisitAsync(TNode node, TCost bestCost, CancellationToken cancellationToken)
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

                var totalCostToNodeViaEdge = bestCost + getEdgeCost(edge);
                var isAlreadyOnFrontier = frontier.TryGetPriority(node, out var frontierDetails);

                if (!isAlreadyOnFrontier && !visited.ContainsKey(node))
                {
                    // Node has not been added to the frontier - add it
                    frontier.Enqueue(node, new (edge, totalCostToNodeViaEdge));
                    visited[node] = new KnownEdgeInfo<TEdge>(edge, true);
                }
                else if (isAlreadyOnFrontier && totalCostToNodeViaEdge < frontierDetails.bestCostToNode)
                {
                    // Node is already on the frontier, but the cost via this edge
                    // is cheaper than has been found previously - update the frontier
                    frontier.IncreasePriority(node, new (edge, totalCostToNodeViaEdge));
                    visited[node] = new KnownEdgeInfo<TEdge>(edge, true);
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
                return y.bestCostToNode.CompareTo(x.bestCostToNode);
            }
        }

        private record struct FrontierNodeInfo(TEdge bestEdgeToNode, TCost bestCostToNode);
    }
}
#endif

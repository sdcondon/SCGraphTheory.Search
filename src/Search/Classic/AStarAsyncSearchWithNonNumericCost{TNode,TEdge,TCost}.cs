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
  /// Implementation of <see cref="IAsyncSearch{TNode, TEdge}"/> that uses the A* algorithm and allows for user-specified non-numeric cost type.
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
  /// <typeparam name="TNode">The node type of the graph being searched.</typeparam>
  /// <typeparam name="TEdge">The edge type of the graph being searched.</typeparam>
  /// <typeparam name="TCost">The type of the cost metric.</typeparam>
  public class AStarAsyncSearchWithNonNumericCost<TNode, TEdge, TCost> : IAsyncSearch<TNode, TEdge>
        where TNode : IAsyncNode<TNode, TEdge>
        where TEdge : IAsyncEdge<TNode, TEdge>
        where TCost : IComparable<TCost>, IComparisonOperators<TCost, TCost, bool>, IAdditionOperators<TCost, TCost, TCost>, IAdditiveIdentity<TCost, TCost>
    {
        private readonly Func<TNode, ValueTask<bool>> isTargetAsync;
        private readonly Func<TEdge, ValueTask<TCost>> getEdgeCostAsync;
        private readonly Func<TNode, ValueTask<TCost>> getEstimatedCostToTargetAsync;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly KeyedPriorityQueue<TNode, FrontierNodeInfo> frontier = new KeyedPriorityQueue<TNode, FrontierNodeInfo>(new FrontierPriorityComparer());

        /// <summary>
        /// Initialises a new instance of the <see cref="AStarAsyncSearchWithNonNumericCost{TNode, TEdge, TCost}"/> class.
        /// </summary>
        /// <param name="isTargetAsync">An async predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCostAsync">An async function for calculating the cost of an edge.</param>
        /// <param name="getEstimatedCostToTargetAsync">An async function for estimating the cost to the target from a given node.</param>
        protected AStarAsyncSearchWithNonNumericCost(
            Func<TNode, ValueTask<bool>> isTargetAsync,
            Func<TEdge, ValueTask<TCost>> getEdgeCostAsync,
            Func<TNode, ValueTask<TCost>> getEstimatedCostToTargetAsync)
        {
            this.isTargetAsync = isTargetAsync ?? throw new ArgumentNullException(nameof(isTargetAsync));
            this.getEdgeCostAsync = getEdgeCostAsync ?? throw new ArgumentNullException(nameof(getEdgeCostAsync));
            this.getEstimatedCostToTargetAsync = getEstimatedCostToTargetAsync ?? throw new ArgumentNullException(nameof(getEstimatedCostToTargetAsync));

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
        /// Creates a new instance of the <see cref="AStarAsyncSearchWithNonNumericCost{TNode, TEdge, TCost}"/> class,
        /// and progresses it to the point at which the nodes adjacent to the source node are on the frontier.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCost">A function for calculating the cost of an edge.</param>
        /// <param name="getEstimatedCostToTarget">A function for estimating the cost to the target from a given node.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask" /> that will return the new search.</returns>
        public static ValueTask<AStarAsyncSearchWithNonNumericCost<TNode, TEdge, TCost>> CreateAsync(
            TNode source,
            Predicate<TNode> isTarget,
            Func<TEdge, TCost> getEdgeCost,
            Func<TNode, TCost> getEstimatedCostToTarget,
            CancellationToken cancellationToken = default)
        {
            return CreateAsync(
                source,
                n => ValueTask.FromResult(isTarget(n)),
                e => ValueTask.FromResult(getEdgeCost(e)),
                n => ValueTask.FromResult(getEstimatedCostToTarget(n)),
                cancellationToken);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AStarAsyncSearchWithNonNumericCost{TNode, TEdge, TCost}"/> class,
        /// and progresses it to the point at which the nodes adjacent to the source node are on the frontier.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTargetAsync">An async predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCostAsync">An async function for calculating the cost of an edge.</param>
        /// <param name="getEstimatedCostToTargetAsync">An async function for estimating the cost to the target from a given node.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask" /> that will return the new search.</returns>
        public static async ValueTask<AStarAsyncSearchWithNonNumericCost<TNode, TEdge, TCost>> CreateAsync(
            TNode source,
            Func<TNode, ValueTask<bool>> isTargetAsync,
            Func<TEdge, ValueTask<TCost>> getEdgeCostAsync,
            Func<TNode, ValueTask<TCost>> getEstimatedCostToTargetAsync,
            CancellationToken cancellationToken = default)
        {
            var search = new AStarAsyncSearchWithNonNumericCost<TNode, TEdge, TCost>(isTargetAsync, getEdgeCostAsync, getEstimatedCostToTargetAsync);
            await search.InitialiseAsync(source, cancellationToken);
            return search;
        }

        /// <inheritdoc />
        public async ValueTask<TEdge> NextStepAsync(CancellationToken cancellationToken)
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var node = frontier.Dequeue(out var frontierInfo);
            visited[node] = new KnownEdgeInfo<TEdge>(frontierInfo.bestEdge, false);
            await VisitAsync(node, frontierInfo.bestCostToNode, cancellationToken);
            return frontierInfo.bestEdge;
        }

        /// <summary>
        /// Initialises the search by conducting the first search step, which adds all of the 
        /// adjacent nodes of a given source node to the search frontier.
        /// </summary>
        /// <param name="source">The source node of the search.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask"/> representing completion of the operation.</returns>
        protected async ValueTask InitialiseAsync(TNode source, CancellationToken cancellationToken = default)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (visited.Count > 0)
            {
                throw new InvalidOperationException("Search already initialised");
            }

            // Initialize the search tree with the source node and immediately visit it.
            // The caller having to do a NextStep to discover it is unintuitive.
            visited[source] = new KnownEdgeInfo<TEdge>(default, false);
            await VisitAsync(source, TCost.AdditiveIdentity, cancellationToken);
        }

        private async ValueTask VisitAsync(TNode node, TCost bestCostToNode, CancellationToken cancellationToken)
        {
            if (await isTargetAsync(node))
            {
                Target = node;
                IsConcluded = true;
                IsSucceeded = true;
                return;
            }

            await foreach (var edge in node.Edges.WithCancellation(cancellationToken))
            {
                node = edge.To;

                var totalCostToNodeViaEdge = bestCostToNode + await getEdgeCostAsync(edge);
                var estimatedTotalCostViaNode = totalCostToNodeViaEdge + await getEstimatedCostToTargetAsync(node);

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

        private record struct FrontierNodeInfo(TEdge bestEdge, TCost bestCostToNode, TCost estimatedBestCostViaNode);
    }
}
#endif
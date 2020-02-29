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

        private readonly Dictionary<TNode, float> costToNode = new Dictionary<TNode, float>();
        private readonly Dictionary<TNode, float> estimatedTotalCostViaNode = new Dictionary<TNode, float>();
        private readonly Dictionary<TNode, TEdge> shortestPathPredecessors = new Dictionary<TNode, TEdge>();
        private readonly Dictionary<TNode, TEdge> frontierPredecessors = new Dictionary<TNode, TEdge>();
        private readonly KeyedPriorityQueue<float, TNode> nodeQueue;

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

            this.costToNode[source] = 0;
            this.estimatedTotalCostViaNode[source] = this.getEstimatedCostToTarget(source);
            this.frontierPredecessors[source] = default;

            // Create an indexed priority queue of nodes. The nodes with the
            // lowest estimated total cost to target via the node are positioned at the front.
            // Put the source node on the queue.
            this.nodeQueue = new KeyedPriorityQueue<float, TNode>((x, y) => y.CompareTo(x));
            nodeQueue.Enqueue(0, source);

            // Immediately add source node to search tree
            NextStep();
        }

        /// <inheritdoc />
        public bool IsConcluded { get; private set; } = false;

        /// <inheritdoc />
        public TNode Target { get; private set; } = default;

        /// <inheritdoc />
        public IReadOnlyDictionary<TNode, TEdge> Predecessors => shortestPathPredecessors;

        /// <summary>
        /// Gets the search frontier - the next nodes (and edges leading to them) under consideration in the search.
        /// </summary>
        public IReadOnlyDictionary<TNode, TEdge> Frontier => frontierPredecessors;

        /// <inheritdoc />
        public void NextStep()
        {
            if (this.IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            // Get the lowest cost node from the queue
            var nextClosestNode = nodeQueue.Dequeue();

            // Move this node from the frontier to the spanning tree
            this.frontierPredecessors.TryGetValue(nextClosestNode, out var predecessor);
            this.shortestPathPredecessors[nextClosestNode] = predecessor;

            // If the target has been found, return
            if (this.isTarget(nextClosestNode))
            {
                this.Target = nextClosestNode;
                this.IsConcluded = true;
                return;
            }

            // Now test all the edges attached to this node
            foreach (var edge in nextClosestNode.Edges)
            {
                var costToNode = this.costToNode[nextClosestNode] + this.getEdgeCost(edge);
                var estimatedTotalCostViaNode = costToNode + this.getEstimatedCostToTarget(edge.To);

                if (!this.frontierPredecessors.ContainsKey(edge.To))
                {
                    // Node has not been added to the frontier - add it and update the costs
                    this.costToNode[edge.To] = costToNode;
                    this.estimatedTotalCostViaNode[edge.To] = estimatedTotalCostViaNode;
                    this.frontierPredecessors[edge.To] = edge;
                    nodeQueue.Enqueue(estimatedTotalCostViaNode, edge.To);
                }
                else if (costToNode < this.costToNode[edge.To] && !this.shortestPathPredecessors.ContainsKey(edge.To))
                {
                    // Node is already on the frontier, but the cost to get here
                    // is cheaper than has been found previously - update the node
                    // costs and frontier accordingly.
                    this.costToNode[edge.To] = costToNode;
                    this.estimatedTotalCostViaNode[edge.To] = estimatedTotalCostViaNode;
                    this.frontierPredecessors[edge.To] = edge;
                    nodeQueue.IncreasePriority(edge.To, estimatedTotalCostViaNode);
                }
            }

            // Check if we've run out of nodes to continue the search with
            if (nodeQueue.Count == 0)
            {
                this.IsConcluded = true;
            }
        }

        /// <summary>
        /// Gets the cost to a particular node (as long as it was visited by the search).
        /// </summary>
        /// <param name="node">The node to get the cost to.</param>
        /// <returns>The cost to the node.</returns>
        public float GetCostTo(TNode node) => this.costToNode[node];
    }
}

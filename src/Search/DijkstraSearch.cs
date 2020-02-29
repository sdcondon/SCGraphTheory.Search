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
        private readonly Dictionary<TNode, TEdge> shortestPathPredecessors = new Dictionary<TNode, TEdge>();
        private readonly Dictionary<TNode, TEdge> frontierPredecessors = new Dictionary<TNode, TEdge>();
        private readonly Dictionary<TNode, float> costs = new Dictionary<TNode, float>();

        private readonly KeyedPriorityQueue<float, TNode> nodeQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DijkstraSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="getEdgeCost">A function for calculating the cost of an edge.</param>
        public DijkstraSearch(TNode source, Predicate<TNode> isTarget, Func<TEdge, float> getEdgeCost)
        {
            this.getEdgeCost = getEdgeCost;
            this.isTarget = isTarget;

            this.costs[source] = 0;
            this.frontierPredecessors[source] = default;

            // Create an indexed priority queue that prioritises by cost, lowest first.
            // Put the source node on the queue
            this.nodeQueue = new KeyedPriorityQueue<float, TNode>((x, y) => y.CompareTo(x));
            this.nodeQueue.Enqueue(0, source);

            if (!IsConcluded)
            {
                // Immediately add source node to search tree
                NextStep();
            }
        }

        /// <inheritdoc />
        public bool IsConcluded { get; private set; } = false;

        /// <inheritdoc />
        public TNode Target { get; private set; }

        /// <inheritdoc />
        public IReadOnlyDictionary<TNode, TEdge> Predecessors => shortestPathPredecessors;

        /// <inheritdoc />
        public void NextStep()
        {
            // while the queue is not empty
            if (this.nodeQueue.Count > 0)
            {
                // get lowest cost node from the queue. This node is the node not already
                // on the SPT that is the closest to the source node
                var nextClosestNode = this.nodeQueue.Dequeue();

                // move this edge from the frontier to the shortest path tree
                this.frontierPredecessors.TryGetValue(nextClosestNode, out var predecessor);
                this.shortestPathPredecessors[nextClosestNode] = predecessor;

                // if the target has been found exit
                if (this.isTarget(nextClosestNode))
                {
                    this.Target = nextClosestNode;
                    this.IsConcluded = true;
                    return;
                }

                // for each edge connected to the next closest node
                foreach (var edge in nextClosestNode.Edges)
                {
                    // the total cost to the node this edge points to is the cost to the
                    // current node plus the cost of the edge connecting them.
                    var newCost = this.costs[nextClosestNode] + this.getEdgeCost(edge);

                    // If this edge has never been on the frontier make a note of the cost
                    // to get to the node it points to, then add the edge to the frontier
                    // and the destination node to the PQ.
                    // Else, test to see if the cost to reach the destination node via the
                    // current node is cheaper than the cheapest cost found so far. If
                    // this path is cheaper, we assign the new cost to the destination
                    // node, update its entry in the PQ to reflect the change and add the
                    // edge to the frontier.
                    if (!this.frontierPredecessors.ContainsKey(edge.To))
                    {
                        this.costs[edge.To] = newCost;
                        this.frontierPredecessors[edge.To] = edge;
                        this.nodeQueue.Enqueue(newCost, edge.To);
                    }
                    else if (newCost < this.costs[edge.To] && !this.shortestPathPredecessors.ContainsKey(edge.To))
                    {
                        this.costs[edge.To] = newCost;
                        this.frontierPredecessors[edge.To] = edge;
                        this.nodeQueue.IncreasePriority(edge.To, newCost);
                    }
                }
            }
            else
            {
                this.IsConcluded = true;
            }
        }

        /// <summary>
        /// Gets the cost to a particular node (as long as it was visited by the search).
        /// </summary>
        /// <param name="node">The node to get the cost to.</param>
        /// <returns>The cost to the node.</returns>
        public float GetCostTo(TNode node) => this.costs[node];
    }
}

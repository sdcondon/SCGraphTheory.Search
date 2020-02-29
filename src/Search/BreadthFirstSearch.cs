using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search
{
    /// <summary>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses the breadth-first search algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    public class BreadthFirstSearch<TNode, TEdge> : ISearch<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
    {
        private readonly Predicate<TNode> isTarget;
        private readonly Queue<KeyValuePair<TNode, TEdge>> edgeQueue;
        private readonly Dictionary<TNode, TEdge> predecessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BreadthFirstSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public BreadthFirstSearch(TNode source, Predicate<TNode> isTarget)
        {
            this.isTarget = isTarget;

            // Create a queue of nodes to consider and the edges that lead us there.
            // Add the source node with a null edge.
            // Strictly speaking we only NEED to keep track of the nodes - but keeping track of the
            // edges that lead us there means we can maintain a full predecessor tree as defined by
            // the ISearch interface, which in turn makes things nice and easy to render.
            edgeQueue = new Queue<KeyValuePair<TNode, TEdge>>();
            edgeQueue.Enqueue(new KeyValuePair<TNode, TEdge>(source, default));

            predecessors = new Dictionary<TNode, TEdge>();

            // Immediately add source node to search tree
            NextStep();
        }

        /// <inheritdoc />
        public bool IsConcluded { get; private set; } = false;

        /// <inheritdoc />
        public TNode Target { get; private set; } = default;

        /// <inheritdoc />
        public IReadOnlyDictionary<TNode, TEdge> Predecessors => predecessors;

        /// <inheritdoc />
        public void NextStep()
        {
            if (this.IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            // Grab the next node and edge leading to it.
            // Add it to the predecessor tree.
            var next = edgeQueue.Dequeue();
            this.predecessors[next.Key] = next.Value;

            // Exit if the target has been found
            if (isTarget(next.Key))
            {
                this.Target = next.Key;
                this.IsConcluded = true;
                return;
            }

            // Push the adjacent nodes (and the edges that lead to them) onto
            // the queue (omitting those already visited).
            foreach (var edge in next.Key.Edges)
            {
                if (!this.predecessors.ContainsKey(edge.To))
                {
                    // BUG/TODO: Avoid null predecessor value here, now that predecessors is exposed publicly.
                    this.predecessors[edge.To] = default;
                    edgeQueue.Enqueue(new KeyValuePair<TNode, TEdge>(edge.To, edge));
                }
            }

            // Check if we've run out of edges to continue to the search with
            if (edgeQueue.Count == 0)
            {
                this.IsConcluded = true;
            }
        }
    }
}

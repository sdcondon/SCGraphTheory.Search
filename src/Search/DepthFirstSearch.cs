using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search
{
    /// <summary>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses the depth-first search algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    public class DepthFirstSearch<TNode, TEdge> : ISearch<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
    {
        private readonly IEnumerator<TNode> sourceEnumerator;
        private readonly Predicate<TNode> isTarget;
        private readonly Stack<KeyValuePair<TNode, TEdge>> stack;
        private readonly Dictionary<TNode, TEdge> predecessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthFirstSearch{TNode, TEdge}"/> class that ensures a whole (even disconnected) graph is searched.
        /// </summary>
        /// <param name="graph">The graph to search.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public DepthFirstSearch(IGraph<TNode, TEdge> graph, Predicate<TNode> isTarget)
        {
            this.sourceEnumerator = graph.Nodes.GetEnumerator();
            this.isTarget = isTarget;
            this.stack = new Stack<KeyValuePair<TNode, TEdge>>();
            this.predecessors = new Dictionary<TNode, TEdge>();

            TryNextSource();
            if (!IsConcluded)
            {
                // Immediately add source node to search tree
                NextStep();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthFirstSearch{TNode, TEdge}"/> class that uses a specified source node.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public DepthFirstSearch(TNode source, Predicate<TNode> isTarget)
        {
            this.sourceEnumerator = ((IEnumerable<TNode>)new TNode[] { source }).GetEnumerator();
            this.isTarget = isTarget;
            this.stack = new Stack<KeyValuePair<TNode, TEdge>>();
            this.predecessors = new Dictionary<TNode, TEdge>();

            TryNextSource();
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
        public IReadOnlyDictionary<TNode, TEdge> Predecessors => this.predecessors;

        /// <inheritdoc />
        public void NextStep()
        {
            if (this.IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            // Grab the next node and edge leading to it.
            // Add it to the predecessor tree.
            var next = stack.Pop();
            this.predecessors[next.Key] = next.Value;

            // If the target has been found the search can return success
            if (isTarget(next.Key))
            {
                this.Target = next.Key;
                this.IsConcluded = true;
                return;
            }

            // Push the adjacent nodes (and the edges that lead to them) onto
            // the stack (omitting those already visited).
            foreach (var edge in next.Key.Edges)
            {
                if (!this.predecessors.ContainsKey(edge.To))
                {
                    // TODO: Explain adding null predecessor value here
                    this.predecessors[edge.To] = default;
                    this.stack.Push(new KeyValuePair<TNode, TEdge>(edge.To, edge));
                }
            }

            // Check if we've run out of nodes to continue the search with
            if (this.stack.Count == 0)
            {
                TryNextSource();
            }
        }

        /// <summary>
        /// Pushes the next source node that has not already been visited onto the stack,
        /// or marks the search as concluded if there are none.
        /// </summary>
        private void TryNextSource()
        {
            while (this.sourceEnumerator.MoveNext())
            {
                if (!this.predecessors.ContainsKey(sourceEnumerator.Current))
                {
                    // Add the source node to the stack with a null edge.
                    // Strictly speaking we only NEED to keep track of the nodes - but keeping track of the
                    // edges that lead us there means we can maintain a full predecessor tree as defined by
                    // the ISearch interface, which in turn makes things nice and easy to render.
                    this.stack.Push(new KeyValuePair<TNode, TEdge>(this.sourceEnumerator.Current, default));
                    break;
                }
            }

            if (this.stack.Count == 0)
            {
                this.IsConcluded = true;
            }
        }
    }
}

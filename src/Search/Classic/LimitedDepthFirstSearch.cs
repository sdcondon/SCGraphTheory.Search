using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses the limited depth-first search algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    public class LimitedDepthFirstSearch<TNode, TEdge> : ISearch<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
    {
        private readonly Predicate<TNode> isTarget;
        private readonly int depthLimit;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly Stack<(TEdge edge, int depth)> frontier = new Stack<(TEdge, int)>();

        private readonly HashSet<TNode> cutoffNodes = new HashSet<TNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitedDepthFirstSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <param name="depthLimit">The depth at which the search should be cut off.</param>
        public LimitedDepthFirstSearch(TNode source, Predicate<TNode> isTarget, int depthLimit)
        {
            // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));
            this.depthLimit = depthLimit;

            Visited = new ReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>>(visited);

            // Initialize the search tree with the source node and immediately visit it.
            // The caller having to do a NextStep to discover it is unintuitive.
            visited[source] = new KnownEdgeInfo<TEdge>(default, false);
            Visit(source, 0);
        }

        /// <summary>
        /// Enumeration of possible states for <see cref="LimitedDepthFirstSearch{TNode, TEdge}"/>.
        /// </summary>
        public enum States
        {
            /// <summary>
            /// The search is ongoing.
            /// </summary>
            InProgress,

            /// <summary>
            /// The search has concluded without finding a target node because there are no more nodes within the depth limit of the source node.
            /// </summary>
            CutOff,

            /// <summary>
            /// The search has concluded without finding a target node even after exploring all nodes within the graph (assuming a connected graph).
            /// </summary>
            Failed,

            /// <summary>
            /// The search has concluded by finding a target node.
            /// </summary>
            Completed,
        }

        /// <summary>
        /// Gets the current state of the search.
        /// </summary>
        public States State { get; private set; } = States.InProgress;

        /// <inheritdoc />
        public bool IsConcluded => State != States.InProgress;

        /// <inheritdoc />
        public bool IsSucceeded => State == States.Completed;

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

            var next = frontier.Pop();
            visited[next.edge.To] = new KnownEdgeInfo<TEdge>(next.edge, false);
            Visit(next.edge.To, next.depth);
            return next.edge;
        }

        private void Visit(TNode node, int depth)
        {
            if (isTarget(node))
            {
                Target = node;
                State = States.Completed;
                return;
            }

            foreach (var nextEdge in node.Edges)
            {
                if (!visited.ContainsKey(nextEdge.To))
                {
                    if (depth < depthLimit)
                    {
                        frontier.Push((nextEdge, depth + 1));
                        visited[nextEdge.To] = new KnownEdgeInfo<TEdge>(nextEdge, true);
                        cutoffNodes.Remove(nextEdge.To);
                    }
                    else
                    {
                        cutoffNodes.Add(nextEdge.To);
                    }
                }
            }

            if (frontier.Count == 0)
            {
                State = cutoffNodes.Count > 0 ? States.CutOff : States.Failed;
            }
        }
    }
}

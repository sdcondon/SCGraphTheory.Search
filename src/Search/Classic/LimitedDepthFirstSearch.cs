using System;
using System.Collections.Generic;

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
        private readonly Stack<(TNode node, TEdge edge, int depth)> frontier = new Stack<(TNode, TEdge, int)>();

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

            // Initialize the frontier with the source node and immediately discover it.
            // The caller having to do a NextStep to discover it is unintuitive.
            frontier.Push((source, default, 0));
            NextStep();
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
            /// The ssearch has concluded by finding a target node.
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

            var next = frontier.Pop();
            visited[next.node] = new KnownEdgeInfo<TEdge>(next.edge, false);

            if (isTarget(next.node))
            {
                Target = next.node;
                State = States.Completed;
                return;
            }

            foreach (var edge in next.node.Edges)
            {
                if (!visited.ContainsKey(edge.To))
                {
                    if (next.depth < depthLimit)
                    {
                        frontier.Push((edge.To, edge, next.depth + 1));
                        visited[edge.To] = new KnownEdgeInfo<TEdge>(edge, true);
                        cutoffNodes.Remove(edge.To);
                    }
                    else
                    {
                        cutoffNodes.Add(edge.To);
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

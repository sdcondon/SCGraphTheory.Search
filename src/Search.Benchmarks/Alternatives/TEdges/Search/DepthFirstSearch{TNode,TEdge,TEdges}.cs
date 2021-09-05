using SCGraphTheory.Search.Classic;
using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.Alternatives.TEdges.Search
{
    /// <summary>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses the depth-first search algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    /// <typeparam name="TEdges">The type of the outbound edges collection of each node of the graph being search.</typeparam>
    public class DepthFirstSearch<TNode, TEdge, TEdges> : ISearch<TNode, TEdge, TEdges>
        where TNode : INode<TNode, TEdge, TEdges>
        where TEdge : IEdge<TNode, TEdge, TEdges>
        where TEdges : IReadOnlyCollection<TEdge>
    {
        private readonly Predicate<TNode> isTarget;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly Stack<TEdge> frontier = new Stack<TEdge>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthFirstSearch{TNode, TEdge, TEdges}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public DepthFirstSearch(TNode source, Predicate<TNode> isTarget)
        {
            // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));

            // Initialize the search tree with the source node and immediately visit it.
            // The caller having to do a NextStep to discover it is unintuitive.
            visited[source] = new KnownEdgeInfo<TEdge>(default, false);
            Visit(source);
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

            var edge = frontier.Pop();
            visited[edge.To] = new KnownEdgeInfo<TEdge>(edge, false);
            Visit(edge.To);
        }

        private void Visit(TNode node)
        {
            if (isTarget(node))
            {
                Target = node;
                IsConcluded = true;
                return;
            }

            foreach (var nextEdge in node.Edges)
            {
                if (!visited.ContainsKey(nextEdge.To))
                {
                    frontier.Push(nextEdge);
                    visited[nextEdge.To] = new KnownEdgeInfo<TEdge>(nextEdge, true);
                }
            }

            if (frontier.Count == 0)
            {
                IsConcluded = true;
            }
        }
    }
}

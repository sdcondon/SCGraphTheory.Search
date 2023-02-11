using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SCGraphTheory.Search.Classic
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
        private readonly Predicate<TNode> isTarget;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly Stack<TEdge> frontier = new Stack<TEdge>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthFirstSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public DepthFirstSearch(TNode source, Predicate<TNode> isTarget)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));

            Visited = new ReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>>(visited);

            // Initialize the search tree with the source node and immediately visit it.
            // The caller having to do a NextStep to discover it is unintuitive.
            visited[source] = new KnownEdgeInfo<TEdge>(default, false);
            Visit(source);
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
        public TEdge NextStep()
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var edge = frontier.Pop();
            visited[edge.To] = new KnownEdgeInfo<TEdge>(edge, false);
            Visit(edge.To);
            return edge;
        }

        private void Visit(TNode node)
        {
            if (isTarget(node))
            {
                Target = node;
                IsConcluded = true;
                IsSucceeded = true;
                return;
            }

            // TODO: I'm really, really tempted to stick a "Reverse()" here. Yes, the fact that we need
            // to iterate the whole collection anyway means its still not great (e.g. if that iteration
            // is expensive), and yes RecursiveDFS exists anyway to account for consumers for whom this
            // is a problem, and yes there could be a better solution where we have a stack of IEnumerators,
            // but it would at least resolve the edges in the reverse order issue - which *is* an issue.
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

﻿using SCGraphTheory.Search.Classic;
using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.AlternativeImplementations.IAltGraph
{
    /// <summary>
    /// Implementation of <see cref="ISearch{TNode, TEdge}"/> that uses the breadth-first search algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    public class BreadthFirstSearch<TNode, TEdge, TEdges>
        where TNode : INode<TNode, TEdge, TEdges>
        where TEdge : IEdge<TNode, TEdge, TEdges>
        where TEdges : IReadOnlyCollection<TEdge>
    {
        private readonly Predicate<TNode> isTarget;

        private readonly Dictionary<TNode, KnownEdgeInfo<TEdge>> visited = new Dictionary<TNode, KnownEdgeInfo<TEdge>>();
        private readonly Queue<(TNode node, TEdge edge)> frontier = new Queue<(TNode, TEdge)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BreadthFirstSearch{TNode, TEdge, TEdges}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public BreadthFirstSearch(TNode source, Predicate<TNode> isTarget)
        {
            // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));

            // Initialize the frontier with the source node and immediately discover it.
            // The caller having to do a NextStep to discover it is unintuitive.
            frontier.Enqueue((source, default));
            NextStep();
        }

        /// <inheritdoc />
        public bool IsConcluded { get; private set; } = false;

        /// <inheritdoc />
        public TNode Target { get; private set; } = default;

        /// <inheritdoc />
        public IReadOnlyDictionary<TNode, KnownEdgeInfo<TEdge>> Visited => visited;

        /// <inheritdoc />
        public void NextStep()
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var next = frontier.Dequeue();
            visited[next.node] = new KnownEdgeInfo<TEdge>(next.edge, false);

            if (isTarget(next.node))
            {
                Target = next.node;
                IsConcluded = true;
                return;
            }

            foreach (var edge in next.node.Edges)
            {
                if (!visited.ContainsKey(edge.To))
                {
                    frontier.Enqueue((edge.To, edge));
                    visited[edge.To] = new KnownEdgeInfo<TEdge>(edge, true);
                }
            }

            if (frontier.Count == 0)
            {
                IsConcluded = true;
            }
        }
    }
}

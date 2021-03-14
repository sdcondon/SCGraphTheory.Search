﻿using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly Dictionary<TNode, TEdge> predecessors = new Dictionary<TNode, TEdge>();
        private readonly Queue<(TNode node, TEdge edge)> frontier = new Queue<(TNode, TEdge)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BreadthFirstSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public BreadthFirstSearch(TNode source, Predicate<TNode> isTarget)
        {
            this.isTarget = isTarget;

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
        public IReadOnlyDictionary<TNode, TEdge> Predecessors => predecessors;

        /// <inheritdoc />
        public void NextStep()
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var next = frontier.Dequeue();
            predecessors[next.node] = next.edge;

            if (isTarget(next.node))
            {
                Target = next.node;
                IsConcluded = true;
                return;
            }

            foreach (var edge in next.node.Edges)
            {
                // NB: Iterating the frontier each time to check for duplicates could be slow. Originally we
                // added nodes on the frontier to the predecessor queue with a edge value of default to
                // avoid this - but of course predecessors is public (and a null edge value in it more ordinarily
                // indicates the source node) so this isn't great. Could have another hashtable field to keep track
                // of this.. Then again, if the frontier is small enough, this might actually be quicker
                // (not that it will be in a BFS - hmm, maybe I should just add another field..).
                if (!predecessors.ContainsKey(edge.To) && !frontier.Any(f => f.node.Equals(edge.To)))
                {
                    frontier.Enqueue((edge.To, edge));
                }
            }

            if (frontier.Count == 0)
            {
                IsConcluded = true;
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.AlternativeSearches.Specialized
{
    /// <summary>
    /// Represents a depth-first search of an and-or graph. Implemented as close as possible to the way it is introduced in §4.3.2 of
    /// "Artificial Intelligence: A Modern Approach", for reference purposes. NB makes the assumption that "or" nodes (i.e. regular
    /// nodes in the usual and-or graph representation) and "and" nodes (that actually represent a set of edges conjoined by an
    /// arc in the usual representation) strictly alternate in the searched graph.
    /// </summary>
    public static class AndOrDFS_FromAIaMA
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AndOrSearch{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from. Is assumed to be an "or" node.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
        /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
        /// <returns>The outcome of the search.</returns>
        public static Outcome<TNode, TEdge> Execute<TNode, TEdge>(TNode source, Predicate<TNode> isTarget)
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
        {
            // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return VisitOrNode<TNode, TEdge>(source, isTarget, Path<TNode>.Empty);
        }

        private static Outcome<TNode, TEdge> VisitOrNode<TNode, TEdge>(TNode orNode, Predicate<TNode> isTarget, Path<TNode> path)
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
        {
            if (isTarget(orNode))
            {
                return new Outcome<TNode, TEdge>(true);
            }

            if (path.Contains(orNode))
            {
                return new Outcome<TNode, TEdge>(false);
            }

            foreach (var edge in orNode.Edges)
            {
                var then = VisitAndNode<TNode, TEdge>(edge.To, isTarget, path.Prepend(orNode));
                if (then != null)
                {
                    return new Outcome<TNode, TEdge>(new Plan<TNode, TEdge>(edge, then));
                }
            }

            return new Outcome<TNode, TEdge>(false);
        }

        private static IReadOnlyDictionary<TNode, Plan<TNode, TEdge>> VisitAndNode<TNode, TEdge>(TNode andNode, Predicate<TNode> isTarget, Path<TNode> path)
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
        {
            var plansByNode = new Dictionary<TNode, Plan<TNode, TEdge>>();

            foreach (var edge in andNode.Edges)
            {
                var outcome = VisitOrNode<TNode, TEdge>(edge.To, isTarget, path);

                if (!outcome.Succeeded)
                {
                    return null;
                }

                plansByNode[edge.To] = outcome.Plan;
            }

            return plansByNode;
        }

        /// <summary>
        /// Container for the outcome of a <see cref="AndOrDFS_FromAIaMA"/> search.
        /// </summary>
        /// <typeparam name="TNode">The node type of the graph searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the graph searched.</typeparam>
        public class Outcome<TNode, TEdge>
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Outcome{TNode, TEdge}"/> class that either indicates failure, or success with an empty plan (because a target node has been reached).
            /// </summary>
            /// <param name="succeeded">A value indicating whether the outcome is a success.</param>
            public Outcome(bool succeeded) => Plan = succeeded ? Plan<TNode, TEdge>.Empty : null;

            /// <summary>
            /// Initializes a new instance of the <see cref="Outcome{TNode, TEdge}"/> class that indicates success.
            /// </summary>
            /// <param name="plan">The plan for reaching the target node or nodes.</param>
            public Outcome(Plan<TNode, TEdge> plan) => Plan = plan ?? throw new ArgumentNullException(nameof(plan));

            /// <summary>
            /// Gets a value indicating whether the search succeeded in creating a plan.
            /// </summary>
            public bool Succeeded => Plan != null;

            /// <summary>
            /// Gets the plan for reaching the target node or nodes.
            /// </summary>
            public Plan<TNode, TEdge> Plan { get; }
        }

        /// <summary>
        /// Container for a plan (or sub-plan) created by a <see cref="AndOrDFS_FromAIaMA"/> search.
        /// </summary>
        /// <typeparam name="TNode">The node type of the graph searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the graph searched.</typeparam>
        public class Plan<TNode, TEdge>
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Plan{TNode, TEdge}"/> class.
            /// </summary>
            /// <param name="first">The edge to follow immediately.</param>
            /// <param name="then">The plan to adhere to after following the edge, keyed by which node we are currently at.</param>
            public Plan(TEdge first, IReadOnlyDictionary<TNode, Plan<TNode, TEdge>> then)
            {
                First = first ?? throw new ArgumentNullException(nameof(first));
                Then = then ?? throw new ArgumentNullException(nameof(then));
            }

            private Plan() => (First, Then) = (default, null);

            /// <summary>
            /// Gets an "empty" plan - that indicates that a target node has been reached.
            /// </summary>
            public static Plan<TNode, TEdge> Empty { get; } = new Plan<TNode, TEdge>();

            /// <summary>
            /// Gets the edge to follow immediately.
            /// </summary>
            public TEdge First { get; }

            /// <summary>
            /// Gets the plan to adhere to after following the edge, keyed by which node we are currently at.
            /// </summary>
            public IReadOnlyDictionary<TNode, Plan<TNode, TEdge>> Then { get; }

            /// <summary>
            /// Flattens the plan out into a single mapping from the current node to the edge that should be followed to ultimately reach a target node or nodes.
            /// Intended to make plans easier to work with in certain situations (e.g. assertions in tests...).
            /// <para/>
            /// Each node will occur at most once in the entire heirarchy of plans, so we can always safely do this.
            /// </summary>
            /// <returns>A mapping from the current node to the edge that should be followed to ultimately reach a target node or nodes.</returns>
            public IReadOnlyDictionary<TNode, TEdge> Flatten()
            {
                var flattened = new Dictionary<TNode, TEdge>();

                void Visit(Plan<TNode, TEdge> plan)
                {
                    if (plan.First != null)
                    {
                        flattened[plan.First.From] = plan.First;
                        foreach (var subPlan in plan.Then.Values)
                        {
                            Visit(subPlan);
                        }
                    }
                }

                Visit(this);

                return flattened;
            }
        }

        private class Path<TNode>
        {
            private Path(TNode first, Path<TNode> rest) => (First, Rest) = (first, rest);

            public static Path<TNode> Empty { get; } = new Path<TNode>(default, null);

            public TNode First { get; }

            public Path<TNode> Rest { get; }

            public Path<TNode> Prepend(TNode node) => new Path<TNode>(node, this);

            public bool Contains(TNode node) => (First?.Equals(node) ?? false) || (Rest?.Contains(node) ?? false);
        }
    }
}

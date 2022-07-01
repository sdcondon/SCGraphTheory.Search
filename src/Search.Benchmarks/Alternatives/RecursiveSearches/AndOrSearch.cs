using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.Alternatives.RecursiveSearches
{
    /// <summary>
    /// Represents a search of an and-or tree.
    /// </summary>
    public static class AndOrSearch
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
            var then = new Dictionary<TNode, Plan<TNode, TEdge>>();

            foreach (var edge in andNode.Edges)
            {
                var outcome = VisitOrNode<TNode, TEdge>(edge.To, isTarget, path);

                if (!outcome.Succeeded)
                {
                    return null;
                }

                then[edge.To] = outcome.Plan;
            }

            return then;
        }

        public class Outcome<TNode, TEdge>
        {
            public Outcome(bool succeeded) => (Succeeded, Plan) = (succeeded, new Plan<TNode, TEdge>(default, null));

            public Outcome(Plan<TNode, TEdge> plan) => (Succeeded, Plan) = (true, plan);

            public bool Succeeded { get; }

            public Plan<TNode, TEdge> Plan { get; }
        }

        public class Plan<TNode, TEdge>
        {
            public Plan(TEdge first, IReadOnlyDictionary<TNode, Plan<TNode, TEdge>> then) => (First, Then) = (first, then);

            public TEdge First { get; }

            public IReadOnlyDictionary<TNode, Plan<TNode, TEdge>> Then { get; }

            /// <summary>
            /// Each node will only occur at most once in the entire heirarchy of plans, so we can flatten to a single dictionary if that's easier to work with.
            /// </summary>
            /// <returns></returns>
            public IReadOnlyDictionary<TNode, TEdge> Flatten(TNode rootNode)
            {
                var flattened = new Dictionary<TNode, TEdge>();

                void Visit(TNode node, Plan<TNode, TEdge> plan)
                {
                    if (plan.First != null)
                    {
                        flattened[node] = plan.First;
                        foreach (var conditional in plan.Then)
                        {
                            Visit(conditional.Key, conditional.Value);
                        }
                    }
                }

                Visit(rootNode, this);

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

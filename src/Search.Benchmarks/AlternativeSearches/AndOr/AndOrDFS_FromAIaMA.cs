using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.AlternativeSearches.AndOr
{
    /// <summary>
    /// Implementation of a depth-first search of an and-or graph. Implemented as close as possible to the way it is introduced in §4.3.2 of
    /// "Artificial Intelligence: A Modern Approach", for reference purposes.
    /// </summary>
    public static class AndOrDFS_FromAIaMA
    {
        /// <summary>
        /// Executes a depth-first search on an and-or graph.
        /// <para/>
        /// NB: Makes the assumption that "or" nodes (i.e. regular nodes in the usual and-or graph representation) and "and" nodes
        /// (that actually represent a set of edges conjoined by an arc in the usual representation - the outbound edges of which
        /// are the actual edges) strictly alternate in the searched graph. This assumption is made even for sets of conjoined edges that
        /// consist of only a single edge. Obviously, this is an easy enough assumption to get rid of, but the point of this implementation
        /// is to be as close as possible to the source material, and to form a baseline.
        /// </summary>
        /// <param name="source">The node to initiate the search from. Is assumed to be an "or" node.</param>
        /// <param name="isTarget">A predicate for identifying the target node(s) of the search.</param>
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

            path = path.Prepend(orNode);

            foreach (var edge in orNode.Edges)
            {
                var subTrees = VisitAndNode<TNode, TEdge>(edge.To, isTarget, path);
                if (subTrees != null)
                {
                    return new Outcome<TNode, TEdge>(new Tree<TNode, TEdge>(edge, subTrees));
                }

                ////foreach (var edge in orNode.Edges)
                ////{
                ////    if (isAndEdgeCollection(edge))
                ////    {
                ////        var subTrees = VisitAndNode(edge.To, path.Prepend(orNode));
                ////        if (subTrees != null)
                ////        {
                ////            return new Outcome(new Tree(edge, subTrees));
                ////        }
                ////    }
                ////    else
                ////    {
                ////        var outcome = VisitOrNode(edge.To, path);
                ////        if (outcome.Succeeded)
                ////        {
                ////            // NB: null-coalescence needed because edge.To might be a target node.
                ////            return new Outcome(new Tree(edge, outcome.Tree.SubTrees ?? new Dictionary<TNode, Tree>()));
                ////        }
                ////    }
                ////}
            }

            return new Outcome<TNode, TEdge>(false);
        }

        // Visits a node that actually represents a set of conjoined edges of a "real" node (assuming the usual representation of and-or graphs).
        private static IReadOnlyDictionary<TNode, Tree<TNode, TEdge>> VisitAndNode<TNode, TEdge>(TNode andNode, Predicate<TNode> isTarget, Path<TNode> path)
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
        {
            var subTrees = new Dictionary<TNode, Tree<TNode, TEdge>>();

            foreach (var edge in andNode.Edges)
            {
                var outcome = VisitOrNode<TNode, TEdge>(edge.To, isTarget, path);

                if (!outcome.Succeeded)
                {
                    return null;
                }

                subTrees[edge.To] = outcome.Tree;
            }

            return subTrees;
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
            /// Initializes a new instance of the <see cref="Outcome{TNode, TEdge}"/> class that either indicates failure, or success with an empty tree (because a target node has been reached).
            /// </summary>
            /// <param name="succeeded">A value indicating whether the outcome is a success.</param>
            internal Outcome(bool succeeded) => Tree = succeeded ? Tree<TNode, TEdge>.Empty : null;

            /// <summary>
            /// Initializes a new instance of the <see cref="Outcome{TNode, TEdge}"/> class that indicates success.
            /// </summary>
            /// <param name="tree">The search tree - the leaves of which include only target nodes.</param>
            internal Outcome(Tree<TNode, TEdge> tree) => Tree = tree ?? throw new ArgumentNullException(nameof(tree));

            /// <summary>
            /// Gets a value indicating whether the search succeeded in creating a tree.
            /// </summary>
            public bool Succeeded => Tree != null;

            /// <summary>
            /// Gets the search tree - the leaves of which include only target nodes.
            /// </summary>
            public Tree<TNode, TEdge> Tree { get; }
        }

        /// <summary>
        /// Container for a tree (or sub-tree) created by a <see cref="AndOrDFS_FromAIaMA"/> search.
        /// </summary>
        /// <typeparam name="TNode">The node type of the graph searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the graph searched.</typeparam>
        public class Tree<TNode, TEdge>
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Tree{TNode, TEdge}"/> class.
            /// </summary>
            /// <param name="root">The root edge of the tree.</param>
            /// <param name="subTreesByRootNode">The sub-trees that follow the root edge, keyed by node that they connect from. There will be more than one if the root edge actually represents a set of more than one coinjoined ("and") edges.</param>
            internal Tree(TEdge root, IReadOnlyDictionary<TNode, Tree<TNode, TEdge>> subTreesByRootNode)
            {
                Root = root ?? throw new ArgumentNullException(nameof(root));
                SubTrees = subTreesByRootNode ?? throw new ArgumentNullException(nameof(subTreesByRootNode));
            }

            // TODO-BUG: We are fine with default structs in the Execute method, but if default is a valid edge then this logic is potentially a (minor) problem..
            private Tree() => (Root, SubTrees) = (default, null);

            /// <summary>
            /// Gets an empty tree - that indicates that a target node has been reached.
            /// </summary>
            public static Tree<TNode, TEdge> Empty { get; } = new Tree<TNode, TEdge>();

            /// <summary>
            /// Gets the root edge of the tree.
            /// </summary>
            public TEdge Root { get; }

            /// <summary>
            /// Gets the sub-trees that follow the root edge, keyed by node that they connect from. There will be more than one if the root edge actually represents a set of more than one coinjoined ("and") edges.
            /// </summary>
            public IReadOnlyDictionary<TNode, Tree<TNode, TEdge>> SubTrees { get; }

            /// <summary>
            /// Flattens the tree out into a single mapping from the current node to the edge that should be followed to ultimately reach only target nodes.
            /// Intended to make trees easier to work with in certain situations (e.g. assertions in tests).
            /// <para/>
            /// Each node will occur at most once in the entire tree, so we can always safely do this.
            /// </summary>
            /// <returns>A mapping from the current node to the edge that should be followed to ultimately reach only target nodes.</returns>
            public IReadOnlyDictionary<TNode, TEdge> Flatten()
            {
                var flattened = new Dictionary<TNode, TEdge>();

                void Visit(Tree<TNode, TEdge> tree)
                {
                    // !tree.Equals(Tree.Empty) might be clearer..?
                    if (tree.Root != null)
                    {
                        flattened[tree.Root.From] = tree.Root;
                        foreach (var subPlan in tree.SubTrees.Values)
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

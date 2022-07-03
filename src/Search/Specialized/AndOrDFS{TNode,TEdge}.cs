using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Benchmarks.AlternativeSearches.Specialized
{
    /// <summary>
    /// Represents a depth-first search of an and-or graph. Implemented as close as possible to the way it is introduced in §4.3.2 of
    /// "Artificial Intelligence: A Modern Approach", for reference purposes.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    public class AndOrDFS<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
    {
        private readonly TNode source;
        private readonly Predicate<TNode> isTarget;
        ////private readonly Predicate<TEdge> isAndEdgeCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndOrDFS{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from. Is assumed to be an "or" node.</param>
        /// <param name="isTarget">A predicate for identifying the target node(s) of the search.</param>
        public AndOrDFS(TNode source, Predicate<TNode> isTarget/*, Predicate<TEdge> isAndEdgeCollection*/)
        {
            // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.source = source;
            this.isTarget = isTarget;
            ////this.isAndEdgeCollection = isAndEdgeCollection;
        }

        /// <summary>
        /// Executes a depth-first search on an and-or graph.
        /// <para/>
        /// NB: Makes the assumption that "or" nodes (i.e. regular nodes in the usual and-or graph representation) and "and" nodes
        /// (that actually represent a set of edges conjoined by an arc in the usual representation - the outbound edges of which
        /// are the actual edges) strictly alternate in the searched graph. This assumption is made even for sets of conjoined edges that
        /// consist of only a single edge. Obviously, this is an easy enough assumption to get rid of, but the point of this implementation
        /// is to be as close as possible to the source material, and to form a baseline.
        /// </summary>
        /// <returns>The outcome of the search.</returns>
        public Outcome Execute()
        {
            return VisitOrNode(source, Path.Empty);
        }

        private Outcome VisitOrNode(TNode orNode, Path path)
        {
            if (isTarget(orNode))
            {
                return new Outcome(true);
            }

            if (path.Contains(orNode))
            {
                return new Outcome(false);
            }

            foreach (var edge in orNode.Edges)
            {
                var then = VisitAndNode(edge.To, path.Prepend(orNode));
                if (then != null)
                {
                    return new Outcome(new Tree(edge, then));
                }

                ////if (isAndEdgeCollection(edge))
                ////{
                ////    var then = VisitAndNode(edge.To, path.Prepend(orNode));
                ////    if (then != null)
                ////    {
                ////        return new Outcome(new Plan(edge, then));
                ////    }
                ////}
                ////else
                ////{
                ////    var outcome = VisitOrNode(edge.To, path);
                ////    if (outcome.Succeeded)
                ////    {
                ////        return ...?
                ////    }
                ////}
            }

            return new Outcome(false);
        }

        // Visits a node that actually represents a set of conjoined edges of a "real" node (assuming the usual representation of and-or graphs).
        private IReadOnlyDictionary<TNode, Tree> VisitAndNode(TNode andNode, Path path)
        {
            var subTreesByNode = new Dictionary<TNode, Tree>();

            foreach (var edge in andNode.Edges)
            {
                var outcome = VisitOrNode(edge.To, path);

                if (!outcome.Succeeded)
                {
                    return null;
                }

                subTreesByNode[edge.To] = outcome.Tree;
            }

            return subTreesByNode;
        }

        /// <summary>
        /// Container for the outcome of a <see cref="AndOrDFS{TNode, TEdge}"/> search.
        /// </summary>
        public class Outcome
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Outcome"/> class that either indicates failure, or success with an empty tree (because a target node has been reached).
            /// </summary>
            /// <param name="succeeded">A value indicating whether the outcome is a success.</param>
            internal Outcome(bool succeeded) => Tree = succeeded ? Tree.Empty : null;

            /// <summary>
            /// Initializes a new instance of the <see cref="Outcome"/> class that indicates success.
            /// </summary>
            /// <param name="tree">The search tree - the leaves of which include only target nodes.</param>
            internal Outcome(Tree tree) => Tree = tree ?? throw new ArgumentNullException(nameof(tree));

            /// <summary>
            /// Gets a value indicating whether the search succeeded in creating a tree.
            /// </summary>
            public bool Succeeded => Tree != null;

            /// <summary>
            /// Gets the search tree - the leaves of which include only target nodes.
            /// </summary>
            public Tree Tree { get; }
        }

        /// <summary>
        /// Container for a tree (or sub-tree) created by a <see cref="AndOrDFS{TNode, TEdge}"/> search.
        /// </summary>
        public class Tree
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Tree"/> class.
            /// </summary>
            /// <param name="first">The edge to follow immediately.</param>
            /// <param name="subTreesByRootNode">The sub-trees that follow the first edge, keyed by node that they connect from. There will be more than one if the first edge actually represents a set of more than one coinjoined ("and") edges.</param>
            public Tree(TEdge first, IReadOnlyDictionary<TNode, Tree> subTreesByRootNode)
            {
                // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
                if (first == null)
                {
                    throw new ArgumentNullException(nameof(first));
                }

                First = first;
                SubTrees = subTreesByRootNode ?? throw new ArgumentNullException(nameof(subTreesByRootNode));
            }

            private Tree() => (First, SubTrees) = (default, null);

            /// <summary>
            /// Gets an empty tree - that indicates that a target node has been reached.
            /// </summary>
            public static Tree Empty { get; } = new Tree();

            /// <summary>
            /// Gets the edge to follow immediately.
            /// </summary>
            public TEdge First { get; }

            /// <summary>
            /// Gets the sub-trees that follow the first edge, keyed by node that they connect from. There will be more than one if the first edge actually represents a set of more than one coinjoined ("and") edges.
            /// </summary>
            public IReadOnlyDictionary<TNode, Tree> SubTrees { get; }

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

                void Visit(Tree tree)
                {
                    if (tree.First != null)
                    {
                        flattened[tree.First.From] = tree.First;
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

        private class Path
        {
            private Path(TNode first, Path rest) => (First, Rest) = (first, rest);

            public static Path Empty { get; } = new Path(default, null);

            public TNode First { get; }

            public Path Rest { get; }

            public Path Prepend(TNode node) => new Path(node, this);

            public bool Contains(TNode node) => (First?.Equals(node) ?? false) || (Rest?.Contains(node) ?? false);
        }
    }
}

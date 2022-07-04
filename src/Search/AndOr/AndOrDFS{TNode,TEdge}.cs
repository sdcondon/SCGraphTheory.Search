using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.AndOr
{
    /// <summary>
    /// Implementation of a depth-first search of an and-or graph.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    public class AndOrDFS<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
    {
        private readonly TNode source;
        private readonly Predicate<TNode> isTarget;
        private readonly Predicate<TEdge> isAndEdgeCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndOrDFS{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from. Is assumed to be an "or" node.</param>
        /// <param name="isTarget">A predicate for identifying the target node(s) of the search.</param>
        /// <param name="isAndEdgeCollection">A predicate for identifying edges that actually represent a conjoined set of "and" edges (that are the outbound edges from the node this edge connects to).</param>
        public AndOrDFS(TNode source, Predicate<TNode> isTarget, Predicate<TEdge> isAndEdgeCollection)
        {
            // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.source = source;
            this.isTarget = isTarget;
            this.isAndEdgeCollection = isAndEdgeCollection;
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
                if (isAndEdgeCollection == null || isAndEdgeCollection(edge))
                {
                    var subTrees = VisitAndNode(edge.To, path.Prepend(orNode));
                    if (subTrees != null)
                    {
                        return new Outcome(new Tree(edge, subTrees));
                    }
                }
                else
                {
                    var outcome = VisitOrNode(edge.To, path);
                    if (outcome.Succeeded)
                    {
                        // NB: null-coalescence needed because edge.To might be a target node.
                        return new Outcome(new Tree(edge, outcome.Tree.SubTrees ?? new Dictionary<TNode, Tree>()));
                    }
                }
            }

            return new Outcome(false);
        }

        // Visits a node that actually represents a set of conjoined edges of a "real" node (assuming the usual representation of and-or graphs).
        private IReadOnlyDictionary<TNode, Tree> VisitAndNode(TNode andNode, Path path)
        {
            var subTrees = new Dictionary<TNode, Tree>();

            foreach (var edge in andNode.Edges)
            {
                var outcome = VisitOrNode(edge.To, path);

                if (!outcome.Succeeded)
                {
                    return null;
                }

                subTrees[edge.To] = outcome.Tree;
            }

            return subTrees;
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
            /// <param name="root">The root edge of the tree.</param>
            /// <param name="subTreesByRootNode">The sub-trees that follow the root edge, keyed by node that they connect from. There will be more than one if the root edge actually represents a set of more than one coinjoined ("and") edges.</param>
            internal Tree(TEdge root, IReadOnlyDictionary<TNode, Tree> subTreesByRootNode)
            {
                // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
                if (root == null)
                {
                    throw new ArgumentNullException(nameof(root));
                }

                Root = root;
                SubTrees = subTreesByRootNode ?? throw new ArgumentNullException(nameof(subTreesByRootNode));
            }

            // TODO-BUG: We are fine with default structs in the search ctor, but if default is a valid edge then this logic is potentially a (minor) problem..
            private Tree() => (Root, SubTrees) = (default, null);

            /// <summary>
            /// Gets an empty tree - that indicates that a target node has been reached.
            /// </summary>
            public static Tree Empty { get; } = new Tree();

            /// <summary>
            /// Gets the root edge of the tree.
            /// </summary>
            public TEdge Root { get; }

            /// <summary>
            /// Gets the sub-trees that follow the root edge, keyed by node that they connect from. There will be more than one if the root edge actually represents a set of more than one coinjoined ("and") edges.
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

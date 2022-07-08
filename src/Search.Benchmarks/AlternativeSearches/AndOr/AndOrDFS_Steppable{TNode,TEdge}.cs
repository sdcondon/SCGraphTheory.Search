using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.AndOr
{
    /// <summary>
    /// Implementation of a depth-first search of an and-or graph that is executable step-by-step.
    /// Slow and ugly. Suspect it could be improved to the point I'm happy with, but.. right now I don't wanna.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    public class AndOrDFS_Steppable<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
    {
        private readonly Predicate<TNode> isTarget;
        private readonly Predicate<TEdge> isAndEdgeCollection;

        private readonly Stack<(IContext cxt, TEdge edge)> frontier = new Stack<(IContext cxt, TEdge edge)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AndOrDFS_Steppable{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from. Is assumed to be an "or" node.</param>
        /// <param name="isTarget">A predicate for identifying the target node(s) of the search.</param>
        /// <param name="isAndEdgeCollection">A predicate for identifying edges that actually represent a conjoined set of "and" edges in the usual and-or graph representation. The actual edges are taken to be the outbound edges from the node this edge connects to.</param>
        public AndOrDFS_Steppable(TNode source, Predicate<TNode> isTarget, Predicate<TEdge> isAndEdgeCollection)
        {
            // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.isTarget = isTarget;
            this.isAndEdgeCollection = isAndEdgeCollection;

            Visit(new RootContext(this), source);
        }

        public bool IsConcluded { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the search succeeded in creating a tree.
        /// </summary>
        public bool Succeeded => Result != null;

        /// <summary>
        /// Gets the search tree - the leaves of which include only target nodes.
        /// </summary>
        public Tree Result { get; private set; }

        public void Complete()
        {
            while (!IsConcluded)
            {
                NextStep();
            }
        }

        /// <summary>
        /// Executes the next step of the search.
        /// </summary>
        public void NextStep()
        {
            if (IsConcluded)
            {
                throw new InvalidOperationException("Search is concluded");
            }

            var (context, node) = frontier.Pop();
            Visit(context, node);
        }

        private void Visit(IContext context, TNode node)
        {
            if (context.Done)
            {
                // Context is an "and" that has failed, or an "or" that has succeeded since this was queued
                return;
            }

            if (isTarget(node))
            {
                context.ReportSuccess(default, Tree.Empty);
                return;
            }

            if (context.PathContains(node))
            {
                context.ReportFailure();
                return;
            }

            var childOrContext = new OrContext(context, node, node.Edges.Count);
            foreach (var childEdge in node.Edges)
            {
                if (isAndEdgeCollection(childEdge))
                {
                    var childAndContext = new AndContext(childOrContext, childEdge, childEdge.To.Edges.Count);
                    foreach (var andEdge in childEdge.To.Edges)
                    {
                        frontier.Push((childAndContext, andEdge));
                    }
                }
                else
                {
                    frontier.Push((childOrContext, childEdge));
                }
            }
        }

        private void Visit(IContext context, TEdge edge)
        {
            if (context.Done)
            {
                // Context is an "and" that has failed, or an "or" that has succeeded since this was queued
                return;
            }

            var node = edge.To;

            if (isTarget(node))
            {
                context.ReportSuccess(edge, Tree.Empty);
                return;
            }

            if (context.PathContains(node))
            {
                context.ReportFailure();
                return;
            }

            var childOrContext = new OrContext(context, edge, node.Edges.Count);
            foreach (var childEdge in node.Edges)
            {
                if (isAndEdgeCollection(childEdge))
                {
                    var childAndContext = new AndContext(childOrContext, childEdge, childEdge.To.Edges.Count);
                    foreach (var andEdge in childEdge.To.Edges)
                    {
                        frontier.Push((childAndContext, andEdge));
                    }
                }
                else
                {
                    frontier.Push((childOrContext, childEdge));
                }
            }
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
                    // BUG: struct edges a problem. !tree.Equals(Tree.Empty) might be nice?
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

        private interface IContext
        {
            bool Done { get; }

            void ReportSuccess(TEdge childEdge, Tree tree);

            void ReportFailure();

            bool PathContains(TNode node);
        }

        private class RootContext : IContext
        {
            private readonly AndOrDFS_Steppable<TNode, TEdge> search;

            public RootContext(AndOrDFS_Steppable<TNode, TEdge> search) => this.search = search;

            public bool Done { get; private set; }

            public bool PathContains(TNode node) => false;

            public void ReportFailure()
            {
                Done = true;
                search.IsConcluded = true;
            }

            public void ReportSuccess(TEdge childEdge, Tree tree)
            {
                Done = true;
                search.IsConcluded = true;
                search.Result = tree;
            }
        }

        private class AndContext : IContext
        {
            private readonly IContext parentContext;
            private readonly TEdge parentEdge;
            private readonly Dictionary<TNode, Tree> subTrees = new Dictionary<TNode, Tree>();
            private int edgeCount;

            public AndContext(OrContext parentContext, TEdge parentEdge, int edgeCount)
            {
                this.parentContext = parentContext;
                this.parentEdge = parentEdge;
                this.edgeCount = edgeCount;
            }

            public bool Done { get; private set; }

            public bool PathContains(TNode node) => parentContext.PathContains(node);

            public void ReportFailure()
            {
                Done = true;
                parentContext.ReportFailure();
            }

            public void ReportSuccess(TEdge childEdge, Tree tree)
            {
                subTrees[childEdge.To] = tree;
                if (--edgeCount == 0)
                {
                    parentContext.ReportSuccess(parentEdge, new Tree(childEdge, subTrees));
                }
            }
        }

        private class OrContext : IContext
        {
            private readonly IContext parentContext;
            private readonly TEdge parentEdge;
            private readonly TNode node;
            private int edgeCount;

            public OrContext(IContext parentContext, TEdge parentEdge, int edgeCount)
                : this(parentContext, parentEdge.To, edgeCount)
            {
                this.parentEdge = parentEdge;
            }

            public OrContext(IContext parentContext, TNode node, int edgeCount)
            {
                this.parentContext = parentContext;
                this.node = node;
                this.edgeCount = edgeCount;
            }

            public bool Done { get; private set; }

            public bool PathContains(TNode node) => this.node.Equals(node) || parentContext.PathContains(node);

            public void ReportFailure()
            {
                if (--edgeCount == 0)
                {
                    Done = true;
                    parentContext.ReportFailure();
                }
            }

            public void ReportSuccess(TEdge childEdge, Tree tree)
            {
                Done = true;
                parentContext.ReportSuccess(parentEdge, new Tree(childEdge, tree.SubTrees ?? new Dictionary<TNode, Tree>()));
            }
        }
    }
}

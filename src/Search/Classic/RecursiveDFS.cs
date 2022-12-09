using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Recursive implementation of a depth-first search. Intended to be of use to those that don't need or want the step-by-step
    /// execution support required by <see cref="ISearch{TNode, TEdge}"/>. By not requiring step-by-step, we eliminate some overhead
    /// (e.g. don't need to enumerate all outbound edges if we find the target via the first one), and explore outbound edges
    /// in order (rather than in reverse order).
    /// <para/>
    /// *Might* implement a compromise at some point that uses a separate stack (so can execute step-by-step) - but the stack stores
    /// enumerators rather than edges. There are some trade-offs there (complexity in establishing the frontier edges at any given
    /// point without breaking the search, for example), but worth a look, maybe.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph to search.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph to search.</typeparam>
    public class RecursiveDFS<TNode, TEdge>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
    {
        private readonly TNode source;
        private readonly Predicate<TNode> isTarget;

        private readonly Dictionary<TNode, TEdge> visited = new Dictionary<TNode, TEdge>();

        private int executeCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecursiveDFS{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public RecursiveDFS(TNode source, Predicate<TNode> isTarget)
        {
            // NB: we don't throw for default structs - which could be valid (struct with a single Id field with value 0, for example)
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.source = source;
            this.isTarget = isTarget ?? throw new ArgumentNullException(nameof(isTarget));

            Visited = new ReadOnlyDictionary<TNode, TEdge>(visited);
            visited[source] = default;
        }

        /// <summary>
        /// Gets a value indicating whether the search is concluded (irrespective of whether a target node was found or not).
        /// </summary>
        public bool IsConcluded { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether the search is concluded, and found a target node.
        /// </summary>
        public bool IsSucceeded { get; private set; } = false;

        /// <summary>
        /// Gets the target node if the search is concluded and found a matching node, otherwise returns <see langword="default"/>.
        /// </summary>
        public TNode Target { get; private set; }

        /// <summary>
        /// Gets the search tree (or forest). Each visited node is present as a key. The associated value is the edge used to discover it (or <see langword="default"/> for the source node).
        /// </summary>
        public IReadOnlyDictionary<TNode, TEdge> Visited { get; }

        /// <summary>
        /// Executes the search to completion.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the operation. Optional, default value is <see cref="CancellationToken.None"/>.</param>
        public void Complete(CancellationToken cancellationToken = default)
        {
            if (Interlocked.Exchange(ref executeCount, 1) == 1)
            {
                throw new InvalidOperationException("Search execution has already begun via a prior Complete invocation");
            }

            Visit(source, cancellationToken);
            IsConcluded = true;
        }

        /// <summary>
        /// Gets an enumeration of the edges comprising the path from the source node to the target - or null if the target was not found.
        /// </summary>
        /// <returns>An enumeration of the edges comprising the path from the source node to the target - or null if the target was not found.</returns>
        public IEnumerable<TEdge> PathToTarget()
        {
            if (Target == null)
            {
                return null;
            }

            var path = new List<TEdge>();

            for (var node = Target; !object.Equals(Visited[node], default(TEdge)); node = Visited[node].From)
            {
                path.Add(Visited[node]);
            }

            // TODO-PERFORMANCE: probably better to use a linked list and continuously add to the front of it. Test me.
            // Then again, this method is unlikely to sit on any hot paths, so probably not a big deal.
            path.Reverse();
            return path;
        }

        private void Visit(TNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (isTarget(node))
            {
                Target = node;
                IsSucceeded = true;
                return;
            }

            var enumerator = node.Edges.GetEnumerator();
            try
            {
                while (!IsSucceeded && enumerator.MoveNext())
                {
                    var nextEdge = enumerator.Current;
                    var nextNode = nextEdge.To;

                    if (!visited.ContainsKey(nextNode))
                    {
                        Visit(nextNode, cancellationToken);
                        visited[nextNode] = nextEdge;
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }
    }
}

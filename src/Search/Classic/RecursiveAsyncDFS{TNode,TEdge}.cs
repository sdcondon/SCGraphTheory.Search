#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic
{
  /// <summary>
  /// Recursive implementation of a depth-first search for async graphs. Intended to be of use to those that don't need or want the
  /// step-by-stepexecution support required by <see cref="IAsyncSearch{TNode, TEdge}"/>. By not requiring step-by-step, we eliminate
  /// some overhead (e.g. don't need to enumerate all outbound edges if we find the target via the first one), and explore outbound
  /// edges in order (rather than in reverse order).
  /// </summary>
  /// <typeparam name="TNode">The node type of the async graph to search.</typeparam>
  /// <typeparam name="TEdge">The edge type of the async graph to search.</typeparam>
  public class RecursiveAsyncDFS<TNode, TEdge>
        where TNode : IAsyncNode<TNode, TEdge>
        where TEdge : IAsyncEdge<TNode, TEdge>
    {
        private readonly TNode source;
        private readonly Predicate<TNode> isTarget;

        private readonly Dictionary<TNode, TEdge> visited = new Dictionary<TNode, TEdge>();

        private int executeCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecursiveAsyncDFS{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="isTarget">A predicate for identifying the target node of the search.</param>
        public RecursiveAsyncDFS(TNode source, Predicate<TNode> isTarget)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
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
        /// <returns>A task representing completion of the operation.</returns>
        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            if (Interlocked.Exchange(ref executeCount, 1) == 1)
            {
                throw new InvalidOperationException("Search execution has already begun via a prior Complete invocation");
            }

            await VisitAsync(source, cancellationToken);
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

        private async ValueTask VisitAsync(TNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (isTarget(node))
            {
                Target = node;
                IsSucceeded = true;
                return;
            }

            var enumerator = node.Edges.GetAsyncEnumerator(cancellationToken);

            while (!IsSucceeded && await enumerator.MoveNextAsync())
            {
                var nextEdge = enumerator.Current;
                var nextNode = nextEdge.To;

                if (!visited.ContainsKey(nextNode))
                {
                    await VisitAsync(nextNode, cancellationToken);
                    visited[nextNode] = nextEdge;
                }
            }
        }
    }
}
#endif

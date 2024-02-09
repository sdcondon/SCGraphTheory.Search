#if NET6_0_OR_GREATER
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Extension methods for the <see cref="IAsyncSearch{TNode, TEdge}"/> interface.
    /// </summary>
    public static class IAsyncSearchExtensions
    {
        /// <summary>
        /// Executes an async search to its conclusion.
        /// </summary>
        /// <typeparam name="TNode">The node type of the async graph being searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the async graph being searched.</typeparam>
        /// <param name="search">The async search to be completed.</param>
        /// <param name="cancellationToken">The cancellation token to respect. Optional, the default value is CancellationToken.None.</param>
        /// <returns>A <see cref="Task"/> encapsulating the completion of the search.</returns>
        public static async Task CompleteAsync<TNode, TEdge>(this IAsyncSearch<TNode, TEdge> search, CancellationToken cancellationToken = default)
            where TNode : IAsyncNode<TNode, TEdge>
            where TEdge : IAsyncEdge<TNode, TEdge>
        {
            while (!search.IsConcluded)
            {
                await search.NextStepAsync(cancellationToken);
            }
        }

        /// <summary>
        /// For a given async search, gets an enumeration of the edges comprising the path from the source node to the target - or null if the target was not found.
        /// </summary>
        /// <typeparam name="TNode">The node type of the async graph being searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the async graph being searched.</typeparam>
        /// <param name="search">The async search to examine.</param>
        /// <returns>For the given search, an enumeration of the edges comprising the path from the source node to the target - or null if the target was not found.</returns>
        public static IEnumerable<TEdge> PathToTarget<TNode, TEdge>(this IAsyncSearch<TNode, TEdge> search)
            where TNode : IAsyncNode<TNode, TEdge>
            where TEdge : IAsyncEdge<TNode, TEdge>
        {
            if (search.Target == null)
            {
                return null;
            }

            var path = new List<TEdge>();

            for (var node = search.Target; !object.Equals(search.Visited[node].Edge, default(TEdge)); node = search.Visited[node].Edge.From)
            {
                path.Add(search.Visited[node].Edge);
            }

            // TODO-PERFORMANCE: probably better to use a linked list and continuously add to the front of it. Test me.
            // Then again, this method is unlikely to sit on any hot paths, so probably not a big deal.
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Gets an enumeration of all edges visited by an async search.
        /// </summary>
        /// <typeparam name="TNode">The node type of the async graph being searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the async graph being searched.</typeparam>
        /// <param name="search">The async search to examine.</param>
        /// <returns>An enumeration of all edges visited by the search.</returns>
        public static IEnumerable<TEdge> VisitedEdges<TNode, TEdge>(this IAsyncSearch<TNode, TEdge> search)
            where TNode : IAsyncNode<TNode, TEdge>
            where TEdge : IAsyncEdge<TNode, TEdge>
        {
            return search.Visited.Values.Where(a => !object.Equals(a.Edge, default(TEdge))).Select(ke => ke.Edge);
        }
    }
}
#endif

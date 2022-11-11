using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic
{
    /// <summary>
    /// Extension methods for the <see cref="ISearch{TNode, TEdge}"/> interface.
    /// </summary>
    public static class ISearchExtensions
    {
        /// <summary>
        /// Executes a search to its conclusion.
        /// </summary>
        /// <typeparam name="TNode">The node type of the graph being searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the graph being searched.</typeparam>
        /// <param name="search">The search to be completed.</param>
        public static void Complete<TNode, TEdge>(this ISearch<TNode, TEdge> search)
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
        {
            while (!search.IsConcluded)
            {
                search.NextStep();
            }
        }

        /// <summary>
        /// Executes a search to its conclusion asynchronously.
        /// <para/>
        /// The <see cref="ISearch{TNode, TEdge}"/> interface of course has no async members (..and more generally, there's no support
        /// for async graphs/graph navigation in this lib). Rather, this extension method is just a quality-of-life addition to let you get a
        /// search going and get on with something else in the meantime. It is implemented such that it should not tie up any given thread
        /// for long periods of time - because it continuously yields back the current sync context (with <see cref="Task.Yield"/>).
        /// This does of course come at an overhead cost - which can be modulated somewhat by providing the frequency with which
        /// it occurs as a value for the 'batchSize' parameter.
        /// </summary>
        /// <typeparam name="TNode">The node type of the graph being searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the graph being searched.</typeparam>
        /// <param name="search">The search to be completed.</param>
        /// <param name="cancellationToken">The cancellation token to respect. Optional, the default value is CancellationToken.None.</param>
        /// <param name="batchSize">The number of steps to attempt between each check of the cancellation token (and yield back to the current sync context). Optional, the default value is 1.</param>
        /// <returns>A <see cref="Task"/> encapsulating the completion of the search.</returns>
        public static async Task CompleteAsync<TNode, TEdge>(this ISearch<TNode, TEdge> search, CancellationToken cancellationToken = default, int batchSize = 1)
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
        {
            var i = 0;

            while (!search.IsConcluded)
            {
                search.NextStep();
                i++;

                // Total paranoia, but based on how careless I've been lately..
                if (i >= batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                    i = 0;
                }
            }
        }

        /// <summary>
        /// For a given search, gets an enumeration of the edges comprising the path from the source node to the target - or null if the target was not found.
        /// </summary>
        /// <typeparam name="TNode">The node type of the graph being searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the graph being searched.</typeparam>
        /// <param name="search">The search to examine.</param>
        /// <returns>For the given search, an enumeration of the edges comprising the path from the source node to the target - or null if the target was not found.</returns>
        public static IEnumerable<TEdge> PathToTarget<TNode, TEdge>(this ISearch<TNode, TEdge> search)
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
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
        /// Gets an enumeration of all edges visited by the search.
        /// </summary>
        /// <typeparam name="TNode">The node type of the graph being searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the graph being searched.</typeparam>
        /// <param name="search">The search to examine.</param>
        /// <returns>An enumeration of all edges visited by the search.</returns>
        public static IEnumerable<TEdge> VisitedEdges<TNode, TEdge>(this ISearch<TNode, TEdge> search)
            where TNode : INode<TNode, TEdge>
            where TEdge : IEdge<TNode, TEdge>
        {
            return search.Visited.Values.Where(a => !object.Equals(a.Edge, default(TEdge))).Select(ke => ke.Edge);
        }
    }
}

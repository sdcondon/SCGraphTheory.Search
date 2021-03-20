using System.Collections.Generic;
using System.Linq;

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

            path.Reverse(); // PERFORMANCE: probably better to use a linked list and continuously add to the front of it
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

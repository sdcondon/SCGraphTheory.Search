using SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Search
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
        /// <typeparam name="TEdges">The type of the outbound edges collection of each node of the graph being search.</typeparam>
        /// <param name="search">The search to be completed.</param>
        public static void Complete<TNode, TEdge, TEdges>(this ISearch<TNode, TEdge, TEdges> search)
            where TNode : INode<TNode, TEdge, TEdges>
            where TEdge : IEdge<TNode, TEdge, TEdges>
            where TEdges : IReadOnlyCollection<TEdge>
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
        /// <typeparam name="TEdges">The type of the outbound edges collection of each node of the graph being search.</typeparam>
        /// <param name="search">The search to examine.</param>
        /// <returns>For the given search, an enumeration of the edges comprising the path from the source node to the target - or null if the target was not found.</returns>
        public static IEnumerable<TEdge> PathToTarget<TNode, TEdge, TEdges>(this ISearch<TNode, TEdge, TEdges> search)
            where TNode : INode<TNode, TEdge, TEdges>
            where TEdge : IEdge<TNode, TEdge, TEdges>
            where TEdges : IReadOnlyCollection<TEdge>
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

            path.Reverse(); // TODO-PERFORMANCE: probably better to use a linked list and continuously add to the front of it. Test me.
            return path;
        }

        /// <summary>
        /// Gets an enumeration of all edges visited by the search.
        /// </summary>
        /// <typeparam name="TNode">The node type of the graph being searched.</typeparam>
        /// <typeparam name="TEdge">The edge type of the graph being searched.</typeparam>
        /// <typeparam name="TEdges">The type of the outbound edges collection of each node of the graph being search.</typeparam>
        /// <param name="search">The search to examine.</param>
        /// <returns>An enumeration of all edges visited by the search.</returns>
        public static IEnumerable<TEdge> VisitedEdges<TNode, TEdge, TEdges>(this ISearch<TNode, TEdge, TEdges> search)
            where TNode : INode<TNode, TEdge, TEdges>
            where TEdge : IEdge<TNode, TEdge, TEdges>
            where TEdges : IReadOnlyCollection<TEdge>
        {
            return search.Visited.Values.Where(a => !object.Equals(a.Edge, default(TEdge))).Select(ke => ke.Edge);
        }
    }
}

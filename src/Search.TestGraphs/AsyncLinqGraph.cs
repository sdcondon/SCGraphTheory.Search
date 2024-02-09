#if NET7_0_OR_GREATER
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.TestGraphs
{
    /// <summary>
    /// Very simple (though rather inefficient) LINQ-powered immutable async graph implementation for use in graph algorithm tests.
    /// </summary>
    public class AsyncLinqGraph : IAsyncGraph<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>
    {
        private readonly Node[] nodes;
        private readonly Edge[] edges;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLinqGraph"/> class.
        /// </summary>
        /// <param name="edges">The edges that comprise the graph, each represented as a tuple indicating the nodes (in turn represented by IDs) that are connected.</param>
        public AsyncLinqGraph(params (int from, int to)[] edges)
            : this(edges.Select(e => (e.from, e.to, 1f)).ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLinqGraph"/> class.
        /// </summary>
        /// <param name="edges">The edges that comprise the graph, each represented as a tuple indicating the nodes (in turn represented by IDs) that are connected, as well as the cost of the edge.</param>
        public AsyncLinqGraph(params (int from, int to, float cost)[] edges)
        {
            this.nodes = edges
                .SelectMany(e => new[] { e.from, e.to })
                .Distinct()
                .Select(i => new Node(this, i))
                .ToArray();

            this.edges = edges
                .Select(e => new Edge(nodes.First(n => n.Id == e.from), nodes.First(n => n.Id == e.to), e.cost))
                .ToArray();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<Node> Nodes => GetNodes();

        /// <inheritdoc />
        public IAsyncEnumerable<Edge> Edges => GetEdges();

        private async IAsyncEnumerable<Node> GetNodes()
        {
            foreach (var node in nodes)
            {
                // make it asynchronous without delaying for any specific length of time
                // should maybe make it occasionally synchronous? Config for this? Meh, fine for now.
                await Task.Yield();
                yield return node;
            }
        }

        private async IAsyncEnumerable<Edge> GetEdges()
        {
            foreach (var edge in edges)
            {
                // make it asynchronous without delaying for any specific length of time
                // should maybe make it occasionally synchronous? Config for this? Meh, fine for now.
                await Task.Yield();
                yield return edge;
            }
        }

        /// <summary>
        /// Node class for <see cref="LinqGraph"/>.
        /// </summary>
        public class Node : IAsyncNode<Node, Edge>
        {
            private readonly AsyncLinqGraph graph;

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class.
            /// </summary>
            /// <param name="graph">The graph that this node is part of.</param>
            /// <param name="id">The unique (within this graph) identifier for this node.</param>
            internal Node(AsyncLinqGraph graph, int id) => (this.graph, Id) = (graph, id);

            /// <summary>
            /// Gets the unique (within this graph) identifier for this node.
            /// </summary>
            public int Id { get; }

            /// <inheritdoc />
            public IAsyncEnumerable<Edge> Edges => graph.Edges.Where(e => e.From.Id == Id);
        }

        /// <summary>
        /// Edge class for <see cref="LinqGraph"/>.
        /// </summary>
        public class Edge : IAsyncEdge<Node, Edge>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Edge"/> class.
            /// </summary>
            /// <param name="from">The node this edge connects from.</param>
            /// <param name="to">The node this edge connects to.</param>
            /// <param name="cost">The cost of this edge.</param>
            internal Edge(Node from, Node to, float cost) => (From, To, Cost) = (from, to, cost);

            /// <inheritdoc />
            public Node From { get; }

            /// <inheritdoc />
            public Node To { get; }

            /// <summary>
            /// Gets the cost of this edge.
            /// </summary>
            public float Cost { get; }
        }
    }
}
#endif

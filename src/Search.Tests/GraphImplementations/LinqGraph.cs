using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Tests.GraphImplementations
{
    /// <summary>
    /// Very simple (though rather inefficient) LINQ-powered immutable graph implementation for use in graph algorithm tests.
    /// </summary>
    public class LinqGraph : IGraph<LinqGraph.Node, LinqGraph.Edge>
    {
        public LinqGraph(params (int from, int to)[] edges)
            : this(edges.Select(e => (e.from, e.to, 1f)).ToArray())
        {
        }

        public LinqGraph(params (int from, int to, float cost)[] edges)
        {
            Nodes = edges.SelectMany(e => new[] { e.from, e.to }).Distinct().Select(i => new Node(this, i)).ToArray();
            Edges = edges.Select(e => new Edge(this, e.from, e.to, e.cost)).ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<Node> Nodes { get; }

        /// <inheritdoc />
        public IEnumerable<Edge> Edges { get; }

        public class Node : INode<Node, Edge>
        {
            private readonly LinqGraph graph;

            public Node(LinqGraph graph, int id) => (this.graph, Id) = (graph, id);

            public int Id { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<Edge> Edges => graph.Edges.Where(e => e.From.Id == Id).ToArray();
        }

        public class Edge : IEdge<Node, Edge>
        {
            private readonly LinqGraph graph;
            private readonly int fromId;
            private readonly int toId;

            public Edge(LinqGraph graph, int fromId, int toId, float cost) => (this.graph, this.fromId, this.toId, this.Cost) = (graph, fromId, toId, cost);

            /// <inheritdoc />
            public Node From => graph.Nodes.Single(n => n.Id == fromId);

            /// <inheritdoc />
            public Node To => graph.Nodes.Single(n => n.Id == toId);

            public float Cost { get; }
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.TestGraphs;

/// <summary>
/// Very simple (though rather inefficient) LINQ-powered immutable graph implementation for use in graph algorithm tests.
/// Uses struct-valued nodes and edges. The motivation here is purely to facilitate testing of algorithms against graphs with
/// value type nodes and edges.
/// </summary>
public class ValLinqGraph : IGraph<ValLinqGraph.Node, ValLinqGraph.Edge>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValLinqGraph"/> class.
    /// </summary>
    /// <param name="edges">The edges that comprise the graph, each represented as a tuple indicating the nodes (in turn represented by IDs) that are connected.</param>
    public ValLinqGraph(params (int from, int to)[] edges)
        : this(edges.Select(e => (e.from, e.to, 1f)).ToArray())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValLinqGraph"/> class.
    /// </summary>
    /// <param name="edges">The edges that comprise the graph, each represented as a tuple indicating the nodes (in turn represented by IDs) that are connected, as well as the cost of the edge.</param>
    public ValLinqGraph(params (int from, int to, float cost)[] edges)
    {
        Nodes = edges.SelectMany(e => new[] { e.from, e.to }).Distinct().Select(i => new Node(this, i)).ToArray();
        Edges = edges.Select(e => new Edge(this, e.from, e.to, e.cost)).ToArray();
    }

    /// <inheritdoc />
    public IEnumerable<Node> Nodes { get; }

    /// <inheritdoc />
    public IEnumerable<Edge> Edges { get; }

    /// <summary>
    /// Gets the node of this graph with a given id.
    /// </summary>
    /// <param name="id">The id of the node to retrieve.</param>
    /// <returns>The node of this graph with the given id.</returns>
    public Node this[int id] => Nodes.Single(n => n.Id == id);

    private IReadOnlyCollection<Edge> GetEdgesFrom(int fromId) => Edges.Where(e => e.From.Id == fromId).ToArray();

    /// <summary>
    /// Node struct for <see cref="LinqGraph"/>.
    /// </summary>
    public readonly struct Node : INode<Node, Edge>
    {
        private readonly ValLinqGraph graph;

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> struct.
        /// </summary>
        /// <param name="graph">The graph that this node is part of.</param>
        /// <param name="id">The unique (within this graph) identifier for this node.</param>
        internal Node(ValLinqGraph graph, int id) => (this.graph, Id) = (graph, id);

        /// <summary>
        /// Gets the unique (within this graph) identifier for this node.
        /// </summary>
        public int Id { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<Edge> Edges => graph.GetEdgesFrom(Id);
    }

    /// <summary>
    /// Edge struct for <see cref="LinqGraph"/>.
    /// </summary>
    public readonly struct Edge : IEdge<Node, Edge>
    {
        private readonly ValLinqGraph graph;
        private readonly int fromId;
        private readonly int toId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge"/> struct.
        /// </summary>
        /// <param name="graph">The graph that this edge is part of.</param>
        /// <param name="fromId">The ID of the node this edge connects from.</param>
        /// <param name="toId">The ID of the node this edge connects to.</param>
        /// <param name="cost">The cost of this edge.</param>
        internal Edge(ValLinqGraph graph, int fromId, int toId, float cost) => (this.graph, this.fromId, this.toId, this.Cost) = (graph, fromId, toId, cost);

        /// <inheritdoc />
        public Node From => graph[fromId];

        /// <inheritdoc />
        public Node To => graph[toId];

        /// <summary>
        /// Gets the cost of this edge.
        /// </summary>
        public float Cost { get; }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SCGraphTheory.Search.TestGraphs;

/// <summary>
/// A very simple (immutable) graph implementation using adjacency matrix representation. Haven't even bothered to add facility for attached data.
/// <para/>
/// Motivation: Just because I was curious to know what an adjacency matrix implementation using the SCGraphTheory abstractions could look like.
/// </summary>
public class AdjacencyMatrixGraph : IGraph<AdjacencyMatrixGraph.Node, AdjacencyMatrixGraph.Edge>
{
    // NB: nodes arguably unneeded - could make Node a struct (though would prob want to make edge collection struct at the same time - and will be boxed by search algs..)
    private readonly Node[] nodes;
    private readonly bool[,] adjacencies;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdjacencyMatrixGraph"/> class.
    /// </summary>
    /// <param name="edges">The edges that comprise the graph, each represented as a tuple indicating the nodes (in turn represented by IDs) that are connected.</param>
    /// <remarks>
    /// Not the best ctor for an AM representation - if the graph is sparse enough that its easy to specify edges as well, a list, AL is a better choice by
    /// definition. But this is just playing..
    /// </remarks>
    public AdjacencyMatrixGraph(params (int from, int to)[] edges)
    {
        // Set up node index:
        // NB: To keep things super simple, we don't even bother being clever about
        // shuffling stuff up if caller-supplied node IDs have any gaps - just
        // make a bigger matrix than we actually need.
        var nodeIndices = edges.SelectMany(e => new[] { e.from, e.to }).Distinct();
        nodes = new Node[nodeIndices.Max() + 1];
        foreach (var nodeIndex in nodeIndices)
        {
            nodes[nodeIndex] = new Node(this, nodeIndex);
        }

        // Set up adjacency matrix:
        adjacencies = new bool[nodes.Length, nodes.Length];
        foreach (var (from, to) in edges)
        {
            adjacencies[from, to] = true;
        }
    }

    /// <inheritdoc />
    public IEnumerable<Node> Nodes => nodes.Where(n => n != null);

    /// <inheritdoc />
    public IEnumerable<Edge> Edges
    {
        get
        {
            foreach (var node in nodes)
            {
                foreach (var edge in node.Edges)
                {
                    yield return edge;
                }
            }
        }
    }

    /// <summary>
    /// Edge type for <see cref="AdjacencyMatrixGraph"/>.
    /// </summary>
    public readonly struct Edge : IEdge<Node, Edge>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Edge"/> struct.
        /// </summary>
        /// <param name="from">The node to connect from.</param>
        /// <param name="to">The node to connect to.</param>
        internal Edge(Node from, Node to) => (From, To) = (from, to);

        /// <inheritdoc />
        public Node From { get; }

        /// <inheritdoc />
        public Node To { get; }
    }

    /// <summary>
    /// Node type for <see cref="AdjacencyMatrixGraph"/>.
    /// </summary>
    public class Node : INode<Node, Edge>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="graph">The graph that the node belongs to.</param>
        /// <param name="index">The index of the node within the graph.</param>
        internal Node(AdjacencyMatrixGraph graph, int index)
        {
            Index = index;
            Edges = new EdgeCollection(graph, this);
        }

        /// <summary>
        /// Gets the index of this node within its graph.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<Edge> Edges { get; }
    }

    private class EdgeCollection : IReadOnlyCollection<Edge>
    {
        private readonly AdjacencyMatrixGraph graph;
        private readonly Node node;

        public EdgeCollection(AdjacencyMatrixGraph graph, Node node) => (this.graph, this.node) = (graph, node);

        /// <inheritdoc />
        [SuppressMessage(
            "Performance",
            "CA1829:Use Length/Count property instead of Count() when available",
            Justification = "False positive. This IS the Count implementation, so using Count would cause infinite recursion.")]
        public int Count => this.Count();

        /// <inheritdoc />
        public IEnumerator<Edge> GetEnumerator()
        {
            for (int i = 0; i < graph.nodes.Length; i++)
            {
                if (graph.adjacencies[node.Index, i])
                {
                    yield return new Edge(node, graph.nodes[i]);
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

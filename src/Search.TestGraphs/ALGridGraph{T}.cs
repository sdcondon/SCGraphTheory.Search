using SCGraphTheory.AdjacencyList;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.TestGraphs
{
    /// <summary>
    /// Graph implementation that wraps a <see cref="SCGraphTheory.AdjacencyList.Graph{TNode, TEdge}"/> and represents a square grid of values, using a delegate to determine which adjacencies are navigable.
    /// </summary>
    /// <typeparam name="T">The type of value associated with each node.</typeparam>
    public class ALGridGraph<T> : IGraph<ALGridGraph<T>.Node, ALGridGraph<T>.Edge>
    {
        private readonly Graph<Node, Edge> innerGraph;
        private readonly Node[,] nodeIndex;
        private readonly Func<T, T, bool> adjacencyIsNavigable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ALGridGraph{T}"/> class initialized with the default value for each node.
        /// </summary>
        /// <param name="size">The size of the grid.</param>
        /// <param name="adjacencyIsNavigable">A predicate to determine whether a given adjacency is navigable (i.e. a given edge exists), based on the value of the adjacent nodes.</param>
        public ALGridGraph((int X, int Y) size, Func<T, T, bool> adjacencyIsNavigable)
        {
            innerGraph = new Graph<Node, Edge>();
            nodeIndex = new Node[size.X, size.Y];
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    innerGraph.Add(nodeIndex[x, y] = new Node(this, (x, y), default));
                }
            }

            this.adjacencyIsNavigable = adjacencyIsNavigable;

            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    SetNodeEdges(x, y);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ALGridGraph{T}"/> class initialized with the default value for each node, and with all adjacencies navigable.
        /// </summary>
        /// <param name="size">The size of the grid.</param>
        public ALGridGraph((int X, int Y) size)
            : this(size, (_, _) => true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ALGridGraph{T}"/> class.
        /// </summary>
        /// <param name="values">The node values for the grid.</param>
        /// <param name="adjacencyIsNavigable">A predicate to determine whether a given adjacency is navigable (i.e. a given edge exists), based on the value of the adjacent nodes.</param>
        public ALGridGraph(T[,] values, Func<T, T, bool> adjacencyIsNavigable)
            : this((values.GetLength(0), values.GetLength(1)), adjacencyIsNavigable)
        {
            for (int x = 0; x < values.GetLength(0); x++)
            {
                for (int y = 0; y < values.GetLength(1); y++)
                {
                    this[x, y].Value = values[x, y];
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ALGridGraph{T}"/> class with all adjacencies navigable.
        /// </summary>
        /// <param name="values">The node values for the grid.</param>
        public ALGridGraph(T[,] values)
            : this(values, (_, _) => true)
        {
        }

        /// <inheritdoc />
        public IEnumerable<Node> Nodes => innerGraph.Nodes;

        /// <inheritdoc />
        public IEnumerable<Edge> Edges => innerGraph.Edges;

        /// <summary>
        /// Indexer of nodes in the graph.
        /// </summary>
        /// <param name="x">The x-ordinate of the node to retrieve.</param>
        /// <param name="y">The y-ordinate of the node to retrieve.</param>
        /// <returns>The graph node with the given coordinates.</returns>
        public Node this[int x, int y] => nodeIndex[x, y];

        private void SetNodeEdges(int x, int y)
        {
            var fromNode = nodeIndex[x, y];

            for (int dx = -1; dx <= 1; dx++)
            {
                if (nodeIndex.GetLowerBound(0) <= x + dx && x + dx <= nodeIndex.GetUpperBound(0))
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if ((dx != 0 || dy != 0) && nodeIndex.GetLowerBound(1) <= y + dy && y + dy <= nodeIndex.GetUpperBound(1))
                        {
                            var toNode = nodeIndex[x + dx, y + dy];
                            var existingEdge = fromNode.Edges.SingleOrDefault(e => e.To.Equals(toNode));
                            var edgeShouldExist = adjacencyIsNavigable(fromNode.Value, toNode.Value);

                            if (edgeShouldExist && existingEdge == null)
                            {
                                innerGraph.Add(new Edge(fromNode, toNode));
                            }
                            else if (!edgeShouldExist && existingEdge != null)
                            {
                                innerGraph.Remove(existingEdge);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Node class for <see cref="ALGridGraph{T}"/>.
        /// </summary>
        public class Node : NodeBase<Node, Edge>
        {
            private readonly ALGridGraph<T> graph;
            private T value;

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class.
            /// </summary>
            /// <param name="graph">The graph that the node lies within.</param>
            /// <param name="coordinates">The coordinates of the node.</param>
            /// <param name="value">The vaue of the node.</param>
            internal Node(ALGridGraph<T> graph, (int X, int Y) coordinates, T value) => (this.graph, Coordinates, this.value) = (graph, coordinates, value);

            /// <summary>
            /// Gets the coordinates of the node.
            /// </summary>
            public (int X, int Y) Coordinates { get; }

            /// <summary>
            /// Gets or sets the value of the node.
            /// </summary>
            public T Value
            {
                get => value;
                set
                {
                    this.value = value;
                    graph.SetNodeEdges(this.Coordinates.X, this.Coordinates.Y);
                }
            }
        }

        /// <summary>
        /// Edge class for <see cref="ALGridGraph{T}"/>.
        /// </summary>
        public class Edge : UndirectedEdgeBase<Node, Edge>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Edge"/> class.
            /// </summary>
            /// <param name="from">The node that the edge connects from.</param>
            /// <param name="to">The node that the edge connects to.</param>
            internal Edge(Node from, Node to)
                : base(from, to, (f, t, r) => new Edge(f, t, r))
            {
            }

            private Edge(Node from, Node to, Edge reverse)
                : base(from, to, reverse)
            {
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.TestGraphs
{
    /// <summary>
    /// Graph implementation for a square grid (with associated values), where the nodes, edges and edge collections are all structs.
    /// </summary>
    /// <typeparam name="T">The type of the values associated with each node of the graph.</typeparam>
    /// <remarks>
    /// Motivation: I wondered to what degree the lack of heap allocations ahead of time would counteract the heavier search load (because more data needs to be copied around).
    /// See the Benchmarks project for results. Generally, I suspect that ALL structs is of limited use, but for graphs with a regular structure (like grids) a hybrid
    /// approach where the nodes are classes but the edges are structs works well. No test graph or benchmark for that as yet though.
    /// </remarks>
    public class ValGridGraph<T> : IGraph<ValGridGraph<T>.Node, ValGridGraph<T>.Edge>
    {
        private readonly T[,] index;
        private readonly Func<T, T, bool> adjacencyIsNavigable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValGridGraph{T}"/> class.
        /// </summary>
        /// <param name="size">The size of the graph.</param>
        /// <param name="adjacencyIsNavigable"></param>
        public ValGridGraph((int X, int Y) size, Func<T, T, bool> adjacencyIsNavigable)
        {
            index = new T[size.X, size.Y];
            this.adjacencyIsNavigable = adjacencyIsNavigable;
        }

        /// <inheritdoc />
        public IEnumerable<Node> Nodes
        {
            get
            {
                for (int x = index.GetLowerBound(0); x <= index.GetLowerBound(0); x++)
                {
                    for (int y = index.GetLowerBound(1); y <= index.GetUpperBound(1); y++)
                    {
                        yield return new Node(this, (x, y));
                    }
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<Edge> Edges
        {
            get
            {
                foreach (var node in Nodes)
                {
                    foreach (var edge in node.Edges)
                    {
                        yield return edge;
                    }
                }
            }
        }

        /// <summary>
        /// Gets an index of node values by position.
        /// </summary>
        /// <param name="x">The x-ordinate of the node to retrieve.</param>
        /// <param name="y">The y-ordinate of the node to retrieve.</param>
        public Node this[int x, int y] => new Node(this, (x, y));

        public void Set(int x, int y, T value)
        {
            index[x, y] = value;
        }

        /// <summary>
        /// Node structure for <see cref="ValGridGraph{T}"/>.
        /// </summary>
        public struct Node : INode<Node, Edge>
        {
            private readonly EdgeCollection edgesPrototype;

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> struct.
            /// </summary>
            /// <param name="graph">The graph that this node belongs to.</param>
            /// <param name="coordinates">The coordinates of the node.</param>
            internal Node(ValGridGraph<T> graph, (int X, int Y) coordinates) => edgesPrototype = new EdgeCollection(graph, coordinates);

            /// <summary>
            /// Gets the coordinates of the node.
            /// </summary>
            public (int X, int Y) Coordinates => edgesPrototype.Coordinates;

            /// <summary>
            /// Gets or sets the value of the node.
            /// </summary>
            public T Value
            {
                get => edgesPrototype.Graph.index[Coordinates.X, Coordinates.Y];
                set => edgesPrototype.Graph.index[Coordinates.X, Coordinates.Y] = value;
            }

            /// <inheritdoc />
            public IReadOnlyCollection<Edge> Edges => edgesPrototype;

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Node n
                && Equals(edgesPrototype.Graph.index, n.edgesPrototype.Graph.index)
                && Equals(Coordinates, n.Coordinates);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(
                edgesPrototype.Graph.index,
                Coordinates);
        }

        /// <summary>
        /// Edge structure for <see cref="ValGridGraph{T}"/>.
        /// </summary>
        public struct Edge : IEdge<Node, Edge>
        {
            internal readonly ValGridGraph<T> Graph;
            internal readonly (int X, int Y) FromCoords;
            internal (sbyte X, sbyte Y) Delta;

            internal Edge(ValGridGraph<T> graph, (int X, int Y) fromCoords, (sbyte X, sbyte Y) d)
            {
                this.Graph = graph;
                this.FromCoords = fromCoords;
                this.Delta = d;
            }

            /// <inheritdoc />
            public Node From => new Node(Graph, FromCoords);

            /// <inheritdoc />
            public Node To => new Node(Graph, (FromCoords.X + Delta.X, FromCoords.Y + Delta.Y));

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Edge e
                && Equals(Graph, e.Graph)
                && Equals(FromCoords, e.FromCoords)
                && Equals(Delta, e.Delta);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(
                Graph,
                FromCoords,
                Delta);
        }

        // NB: Used via its interface in search algorithms, so presumably will be getting boxed :(
        private struct EdgeCollection : IReadOnlyCollection<Edge>
        {
            internal readonly ValGridGraph<T> Graph;
            internal readonly (int X, int Y) Coordinates;

            internal EdgeCollection(ValGridGraph<T> graph, (int X, int Y) coordinates) => (Graph, Coordinates) = (graph, coordinates);

            public int Count => this.Count();

            public IEnumerator<Edge> GetEnumerator() => new EdgeEnumerator(Graph, Coordinates);

            IEnumerator IEnumerable.GetEnumerator() => new EdgeEnumerator(Graph, Coordinates);
        }

        private struct EdgeEnumerator : IEnumerator<Edge>
        {
            private Edge currentPrototype;

            internal EdgeEnumerator(ValGridGraph<T> graph, (int X, int Y) coordinates)
            {
                currentPrototype = new Edge(graph, coordinates, (-2, -1));
            }

            public Edge Current => currentPrototype;

            object IEnumerator.Current => currentPrototype;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                do
                {
                    currentPrototype.Delta.X++;
                    if (currentPrototype.Delta.X > 1)
                    {
                        currentPrototype.Delta.X = -1;
                        currentPrototype.Delta.Y++;
                        if (currentPrototype.Delta.Y > 1)
                        {
                            return false;
                        }
                    }
                }
                while (
                    currentPrototype.FromCoords.X + currentPrototype.Delta.X < currentPrototype.Graph.index.GetLowerBound(0)
                    || currentPrototype.FromCoords.X + currentPrototype.Delta.X > currentPrototype.Graph.index.GetUpperBound(0)
                    || currentPrototype.FromCoords.Y + currentPrototype.Delta.Y < currentPrototype.Graph.index.GetLowerBound(1)
                    || currentPrototype.FromCoords.Y + currentPrototype.Delta.Y > currentPrototype.Graph.index.GetUpperBound(1)
                    || currentPrototype.Delta == (0, 0)
                    || !currentPrototype.Graph.adjacencyIsNavigable(currentPrototype.Graph.index[currentPrototype.FromCoords.X, currentPrototype.FromCoords.Y], currentPrototype.Graph.index[currentPrototype.FromCoords.X + currentPrototype.Delta.X, currentPrototype.FromCoords.Y + currentPrototype.Delta.Y]));

                return true;
            }

            public void Reset() => currentPrototype.Delta = (-2, -1);
        }
    }
}

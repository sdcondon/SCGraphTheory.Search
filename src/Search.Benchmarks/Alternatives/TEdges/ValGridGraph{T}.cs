using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Benchmarks.Alternatives.TEdges
{
    /// <summary>
    /// Graph implementation for a square grid (with associated values), where the nodes, edges and edge collections are all structs.
    /// </summary>
    /// <typeparam name="T">The type of the values associated with each node of the graph.</typeparam>
    /// <remarks>
    /// Motivation: I wondered to what degree the lack of heap allocations ahead of time would counteract the heavier search load (because
    /// more data needs to be copied around). Run the Benchmarks project for results. Generally, I suspect that ALL structs is of limited
    /// use, but for graphs with a regular structure (like grids) a hybrid approach where the nodes are classes but the edges are structs
    /// would work well. No test graph or benchmark for that as yet though.
    /// </remarks>
    public class ValGridGraph<T> : IGraph<ValGridGraph<T>.Node, ValGridGraph<T>.Edge, ValGridGraph<T>.EdgeCollection>
    {
        private readonly T[,] index;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValGridGraph{T}"/> class.
        /// </summary>
        /// <param name="size">The size of the graph.</param>
        public ValGridGraph((int X, int Y) size) => index = new T[size.X, size.Y];

        /// <inheritdoc />
        public IEnumerable<Node> Nodes
        {
            get
            {
                for (int x = index.GetLowerBound(0); x <= index.GetUpperBound(0); x++)
                {
                    for (int y = index.GetLowerBound(1); y <= index.GetUpperBound(1); y++)
                    {
                        yield return new Node(index, (x, y));
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
        public Node this[int x, int y] => new Node(index, (x, y));

        /// <summary>
        /// Node structure for <see cref="ValGridGraph{T}"/>.
        /// </summary>
        public struct Node : INode<Node, Edge, EdgeCollection>
        {
            private readonly T[,] index;
            private readonly (int X, int Y) coordinates;

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> struct.
            /// </summary>
            /// <param name="index">The index of node values to use when getting or setting node value.</param>
            /// <param name="coordinates">The coordinates of the node.</param>
            internal Node(T[,] index, (int X, int Y) coordinates) => (this.index, this.coordinates) = (index, coordinates);

            /// <summary>
            /// Gets the coordinates of the node.
            /// </summary>
            public (int X, int Y) Coordinates => coordinates;

            /// <summary>
            /// Gets or sets the value of the node.
            /// </summary>
            public T Value
            {
                get => index[coordinates.X, coordinates.Y];
                set => index[coordinates.X, coordinates.Y] = value;
            }

            /// <inheritdoc />
            public EdgeCollection Edges => new EdgeCollection(index, coordinates);

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Node n
                && Equals(index, n.index)
                && coordinates.Equals(n.Coordinates);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(
                index,
                coordinates);
        }

        /// <summary>
        /// Edge structure for <see cref="ValGridGraph{T}"/>.
        /// </summary>
        public struct Edge : IEdge<Node, Edge, EdgeCollection>
        {
            private readonly T[,] index;
            private readonly (int X, int Y) fromCoords;
            private readonly (sbyte X, sbyte Y) delta;

            internal Edge(T[,] index, (int X, int Y) fromCoords, (sbyte X, sbyte Y) d)
            {
                this.index = index;
                this.fromCoords = fromCoords;
                this.delta = d;
            }

            /// <inheritdoc />
            public Node From => new Node(index, fromCoords);

            /// <inheritdoc />
            public Node To => new Node(index, (fromCoords.X + delta.X, fromCoords.Y + delta.Y));

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Edge e
                && Equals(index, e.index)
                && fromCoords.Equals(e.fromCoords)
                && delta.Equals(e.delta);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(
                index,
                fromCoords,
                delta);
        }

        // NB: Used via its interface in search algorithms, so presumably will be getting boxed :(
        public struct EdgeCollection : IReadOnlyCollection<Edge>
        {
            private readonly T[,] index;
            private readonly (int X, int Y) coordinates;

            internal EdgeCollection(T[,] index, (int X, int Y) coordinates) => (this.index, this.coordinates) = (index, coordinates);

            public int Count => this.Count();

            public EdgeEnumerator GetEnumerator() => new EdgeEnumerator(index, coordinates);

            IEnumerator<Edge> IEnumerable<Edge>.GetEnumerator() => new EdgeEnumerator(index, coordinates);

            IEnumerator IEnumerable.GetEnumerator() => new EdgeEnumerator(index, coordinates);
        }

        public struct EdgeEnumerator : IEnumerator<Edge>
        {
            private readonly T[,] index;
            private readonly (int X, int Y) coordinates;
            private (sbyte X, sbyte Y) currentDelta;

            internal EdgeEnumerator(T[,] index, (int X, int Y) coordinates)
            {
                this.index = index;
                this.coordinates = coordinates;
                this.currentDelta = (-2, -1);
            }

            // NB: A bug here - getting Current before MoveNext gives a wrong edge instead of throwing.
            public Edge Current => new Edge(index, coordinates, currentDelta);

            object IEnumerator.Current => new Edge(index, coordinates, currentDelta);

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                do
                {
                    currentDelta.X++;
                    if (currentDelta.X > 1)
                    {
                        currentDelta.X = -1;
                        currentDelta.Y++;
                        if (currentDelta.Y > 1)
                        {
                            return false;
                        }
                    }
                }
                while (
                    coordinates.X + currentDelta.X < index.GetLowerBound(0)
                    || coordinates.X + currentDelta.X > index.GetUpperBound(0)
                    || coordinates.Y + currentDelta.Y < index.GetLowerBound(1)
                    || coordinates.Y + currentDelta.Y > index.GetUpperBound(1)
                    || currentDelta == (0, 0));

                return true;
            }

            public void Reset() => currentDelta = (-2, -1);
        }
    }
}

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
    /// Motivation: I wondered if the lack of heap allocations ahead of time would outweigh the heavier search load (because more data needs to be copied around).
    /// See the Benchmarks project for results..
    /// </remarks>
    public class ValGridGraph<T> : IGraph<ValGridGraph<T>.Node, ValGridGraph<T>.Edge>
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
                for (int x = index.GetLowerBound(0); x <= index.GetLowerBound(0); x++)
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
        /// <remarks>
        /// Note how we retrieve the index and coordinates from the edge collection prototype rather than storing it twice - to ensure the struct is as small as it can be.</remarks>
        /// </remarks>
        public struct Node : INode<Node, Edge>
        {
            private readonly EdgeCollection edgesPrototype;

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> struct.
            /// </summary>
            /// <param name="index">The index of node values to use when getting or setting node value.</param>
            /// <param name="coordinates">The coordinates of the node.</param>
            internal Node(T[,] index, (int X, int Y) coordinates) => edgesPrototype = new EdgeCollection(index, coordinates);

            /// <summary>
            /// Gets the coordinates of the node.
            /// </summary>
            public (int X, int Y) Coordinates => edgesPrototype.EnumeratorPrototype.CurrentPrototype.FromCoords;

            /// <summary>
            /// Gets or sets the value of the node.
            /// </summary>
            public T Value
            {
                get => edgesPrototype.EnumeratorPrototype.CurrentPrototype.Index[Coordinates.X, Coordinates.Y];
                set => edgesPrototype.EnumeratorPrototype.CurrentPrototype.Index[Coordinates.X, Coordinates.Y] = value;
            }

            /// <inheritdoc />
            public IReadOnlyCollection<Edge> Edges => edgesPrototype;

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Node n
                && Equals(edgesPrototype.EnumeratorPrototype.CurrentPrototype.Index, n.edgesPrototype.EnumeratorPrototype.CurrentPrototype.Index)
                && Equals(Coordinates, n.Coordinates);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(
                edgesPrototype.EnumeratorPrototype.CurrentPrototype.Index,
                Coordinates);
        }

        /// <summary>
        /// Edge structure for <see cref="ValGridGraph{T}"/>.
        /// </summary>
        public struct Edge : IEdge<Node, Edge>
        {
            internal readonly T[,] Index;
            internal readonly (int X, int Y) FromCoords;
            internal (sbyte X, sbyte Y) Delta;

            internal Edge(T[,] index, (int X, int Y) fromCoords, (sbyte X, sbyte Y) d)
            {
                this.Index = index;
                this.FromCoords = fromCoords;
                this.Delta = d;
            }

            /// <inheritdoc />
            public Node From => new Node(Index, FromCoords);

            /// <inheritdoc />
            public Node To => new Node(Index, (FromCoords.X + Delta.X, FromCoords.Y + Delta.Y));

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Edge e
                && Equals(Index, e.Index)
                && Equals(FromCoords, e.FromCoords)
                && Equals(Delta, e.Delta);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(
                Index,
                FromCoords,
                Delta);
        }

        private struct EdgeCollection : IReadOnlyCollection<Edge>
        {
            internal readonly EdgeEnumerator EnumeratorPrototype;

            internal EdgeCollection(T[,] index, (int X, int Y) coordinates)
            {
                EnumeratorPrototype = new EdgeEnumerator(index, coordinates);
            }

            public int Count => this.Count();

            public IEnumerator<Edge> GetEnumerator() => EnumeratorPrototype;

            IEnumerator IEnumerable.GetEnumerator() => EnumeratorPrototype;
        }

        private struct EdgeEnumerator : IEnumerator<Edge>
        {
            internal Edge CurrentPrototype;

            internal EdgeEnumerator(T[,] index, (int X, int Y) coordinates)
            {
                CurrentPrototype = new Edge(index, coordinates, (-2, -1));
            }

            public Edge Current => CurrentPrototype;

            object IEnumerator.Current => CurrentPrototype;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                do
                {
                    CurrentPrototype.Delta.X++;
                    if (CurrentPrototype.Delta.X > 1)
                    {
                        CurrentPrototype.Delta.X = -1;
                        CurrentPrototype.Delta.Y++;
                        if (CurrentPrototype.Delta.Y > 1)
                        {
                            return false;
                        }
                    }
                }
                while (
                    CurrentPrototype.FromCoords.X + CurrentPrototype.Delta.X < CurrentPrototype.Index.GetLowerBound(0)
                    || CurrentPrototype.FromCoords.X + CurrentPrototype.Delta.X > CurrentPrototype.Index.GetUpperBound(0)
                    || CurrentPrototype.FromCoords.Y + CurrentPrototype.Delta.Y < CurrentPrototype.Index.GetLowerBound(1)
                    || CurrentPrototype.FromCoords.Y + CurrentPrototype.Delta.Y > CurrentPrototype.Index.GetUpperBound(1)
                    || CurrentPrototype.Delta == (0, 0));

                return true;
            }

            public void Reset() => CurrentPrototype.Delta = (-2, -1);
        }
    }
}

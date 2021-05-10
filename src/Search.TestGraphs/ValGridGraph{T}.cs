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
            public (int X, int Y) Coordinates => edgesPrototype.Coordinates;

            /// <summary>
            /// Gets or sets the value of the node.
            /// </summary>
            public T Value
            {
                get => edgesPrototype.Index[Coordinates.X, Coordinates.Y];
                set => edgesPrototype.Index[Coordinates.X, Coordinates.Y] = value;
            }

            /// <inheritdoc />
            public IReadOnlyCollection<Edge> Edges => edgesPrototype;

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Node n
                && Equals(edgesPrototype.Index, n.edgesPrototype.Index)
                && Equals(Coordinates, n.Coordinates);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(
                edgesPrototype.Index,
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

        // NB: Used via its interface in search algorithms, so presumably will be getting boxed :(
        private struct EdgeCollection : IReadOnlyCollection<Edge>
        {
            internal readonly T[,] Index;
            internal readonly (int X, int Y) Coordinates;

            internal EdgeCollection(T[,] index, (int X, int Y) coordinates) => (Index, Coordinates) = (index, coordinates);

            public int Count => this.Count();

            public IEnumerator<Edge> GetEnumerator() => new EdgeEnumerator(Index, Coordinates);

            IEnumerator IEnumerable.GetEnumerator() => new EdgeEnumerator(Index, Coordinates);
        }

        private struct EdgeEnumerator : IEnumerator<Edge>
        {
            private Edge currentPrototype;

            internal EdgeEnumerator(T[,] index, (int X, int Y) coordinates)
            {
                currentPrototype = new Edge(index, coordinates, (-2, -1));
            }

            // NB: A bug here - getting Current before MoveNext gives a wrong edge instead of throwing.
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
                    currentPrototype.FromCoords.X + currentPrototype.Delta.X < currentPrototype.Index.GetLowerBound(0)
                    || currentPrototype.FromCoords.X + currentPrototype.Delta.X > currentPrototype.Index.GetUpperBound(0)
                    || currentPrototype.FromCoords.Y + currentPrototype.Delta.Y < currentPrototype.Index.GetLowerBound(1)
                    || currentPrototype.FromCoords.Y + currentPrototype.Delta.Y > currentPrototype.Index.GetUpperBound(1)
                    || currentPrototype.Delta == (0, 0));

                return true;
            }

            public void Reset() => currentPrototype.Delta = (-2, -1);
        }
    }
}

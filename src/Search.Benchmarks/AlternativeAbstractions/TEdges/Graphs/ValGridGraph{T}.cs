﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Benchmarks.AlternativeAbstractions.TEdges.Graphs
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
        private readonly T[,] values;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValGridGraph{T}"/> class.
        /// </summary>
        /// <param name="size">The size of the graph.</param>
        public ValGridGraph((int X, int Y) size) => values = new T[size.X, size.Y];

        /// <inheritdoc />
        public IEnumerable<Node> Nodes
        {
            get
            {
                for (int x = values.GetLowerBound(0); x <= values.GetUpperBound(0); x++)
                {
                    for (int y = values.GetLowerBound(1); y <= values.GetUpperBound(1); y++)
                    {
                        yield return new Node(values, (x, y));
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
        public Node this[int x, int y] => new Node(values, (x, y));

        /// <summary>
        /// Node structure for <see cref="ValGridGraph{T}"/>.
        /// </summary>
        public struct Node : INode<Node, Edge, EdgeCollection>, IEquatable<Node>
        {
            private readonly T[,] values;
            private readonly (int X, int Y) coordinates;

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> struct.
            /// </summary>
            /// <param name="values">The index of node values to use when getting or setting node value.</param>
            /// <param name="coordinates">The coordinates of the node.</param>
            internal Node(T[,] values, (int X, int Y) coordinates) => (this.values, this.coordinates) = (values, coordinates);

            /// <summary>
            /// Gets the coordinates of the node.
            /// </summary>
            public (int X, int Y) Coordinates => coordinates;

            /// <summary>
            /// Gets or sets the value of the node.
            /// </summary>
            public T Value
            {
                get => values[coordinates.X, coordinates.Y];
                set => values[coordinates.X, coordinates.Y] = value;
            }

            /// <inheritdoc />
            public EdgeCollection Edges => new EdgeCollection(values, coordinates);

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Node n && Equals(n);

            /// <inheritdoc />
            public bool Equals(Node other) => Equals(values, other.values) && coordinates.Equals(other.Coordinates);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(values, coordinates);
        }

        /// <summary>
        /// Edge structure for <see cref="ValGridGraph{T}"/>.
        /// </summary>
        public struct Edge : IEdge<Node, Edge, EdgeCollection>, IEquatable<Edge>
        {
            private readonly T[,] values;
            private readonly (int X, int Y) fromCoords;
            private readonly (sbyte X, sbyte Y) delta;

            internal Edge(T[,] values, (int X, int Y) fromCoords, (sbyte X, sbyte Y) d)
            {
                this.values = values;
                this.fromCoords = fromCoords;
                delta = d;
            }

            /// <inheritdoc />
            public Node From => new Node(values, fromCoords);

            /// <inheritdoc />
            public Node To => new Node(values, (fromCoords.X + delta.X, fromCoords.Y + delta.Y));

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Edge e
                && Equals(e);

            /// <inheritdoc />
            public bool Equals(Edge other) => Equals(values, other.values)
                && fromCoords.Equals(other.fromCoords)
                && delta.Equals(other.delta);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(
                values,
                fromCoords,
                delta);
        }

        // NB: Used via its interface in search algorithms, so presumably will be getting boxed :(
        public struct EdgeCollection : IReadOnlyCollection<Edge>
        {
            private readonly T[,] values;
            private readonly (int X, int Y) coordinates;

            internal EdgeCollection(T[,] values, (int X, int Y) coordinates) => (this.values, this.coordinates) = (values, coordinates);

            /// <inheritdoc />
            public int Count => this.Count();

            public EdgeEnumerator GetEnumerator() => new EdgeEnumerator(values, coordinates);

            /// <inheritdoc />
            IEnumerator<Edge> IEnumerable<Edge>.GetEnumerator() => new EdgeEnumerator(values, coordinates);

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => new EdgeEnumerator(values, coordinates);
        }

        public struct EdgeEnumerator : IEnumerator<Edge>
        {
            // We use an offset lookup rather than calculating them each time, for performance
            private static readonly (sbyte X, sbyte Y)[] Offsets = new (sbyte, sbyte)[]
            {
                (-1, -1), (0, -1), (1, -1),
                (-1,  0),          (1,  0),
                (-1,  1), (0,  1), (1,  1),
            };

            private readonly T[,] values;
            private readonly (int X, int Y) coordinates;
            private sbyte index = -1;

            internal EdgeEnumerator(T[,] values, (int X, int Y) coordinates)
            {
                this.values = values;
                this.coordinates = coordinates;
            }

            /// <inheritdoc />
            /// <remarks>
            /// NB: Bugs here - getting Current before MoveNext gives a wrong edge instead of throwing.
            /// Is also wrong if we've reached end of enumeration.
            /// </remarks>
            public Edge Current => new Edge(values, coordinates, Offsets[index]);

            /// <inheritdoc />
            object IEnumerator.Current => new Edge(values, coordinates, Offsets[index]);

            /// <inheritdoc />
            public void Dispose()
            {
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                // TODO-PERFORMANCE: Might try out 16 different look-ups at some point
                // (one for each min/max x/y combo) - to eliminate the need for the while loop.
                (int X, int Y) current;
                var maxX = values.GetUpperBound(0);
                var maxY = values.GetUpperBound(1);
                do
                {
                    if (++index > 7)
                    {
                        return false;
                    }

                    current = (coordinates.X + Offsets[index].X, coordinates.Y + Offsets[index].Y);
                }
                while (
                    current.X < 0
                    || current.X > maxX
                    || current.Y < 0
                    || current.Y > maxY);

                return true;
            }

            /// <inheritdoc />
            public void Reset() => index = -1;
        }
    }
}

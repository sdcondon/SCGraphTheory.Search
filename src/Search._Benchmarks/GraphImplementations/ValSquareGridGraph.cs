using SCGraphTheory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Search.Benchmarks.GraphImplementations
{
    /// <summary>
    /// Graph implementation for a square grid (with associated values), where the nodes, edges and edge collections are all structs.
    /// Wondered if the lack of heap allocations ahead of time would outweigh the heavier search load (because more data is copied around).
    /// </summary>
    public class ValSquareGridGraph<T> : IGraph<ValSquareGridGraph<T>.Node, ValSquareGridGraph<T>.Edge>
    {
        private readonly T[,] index;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValSquareGridGraph{T}"/> class.
        /// </summary>
        /// <param name="size">The size of the graph.</param>
        public ValSquareGridGraph((int X, int Y) size) => index = new T[size.X, size.Y];

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
        public Node this[int x, int y] => new Node(index, (x, y));

        public struct Node : INode<Node, Edge>
        {
            private readonly TileAdjacencyCollection adjacenciesPrototype;

            internal Node(T[,] index, (int X, int Y) coordinates) => adjacenciesPrototype = new TileAdjacencyCollection(index, coordinates);

            /// <summary>
            /// Gets the coordinates of the node.
            /// </summary>
            /// <remarks>We don't store coordinates again in the node struct to not double up on storing it.</remarks>
            public (int X, int Y) Coordinates => adjacenciesPrototype.enumeratorPrototype.current.from;

            /// <summary>
            /// Gets or sets the value of the node.
            /// </summary>
            /// <remarks>We don't store index again in the node struct to not double up on storing it.</remarks>
            public T Value
            {
                get => adjacenciesPrototype.enumeratorPrototype.current.index[Coordinates.X, Coordinates.Y];
                set => adjacenciesPrototype.enumeratorPrototype.current.index[Coordinates.X, Coordinates.Y] = value;
            }

            /// <inheritdoc />
            public IReadOnlyCollection<Edge> Edges => adjacenciesPrototype;

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Node n
                && Equals(adjacenciesPrototype.enumeratorPrototype.current.index, n.adjacenciesPrototype.enumeratorPrototype.current.index)
                && Equals(Coordinates, n.Coordinates);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(
                adjacenciesPrototype.enumeratorPrototype.current.index,
                Coordinates);
        }

        public struct Edge : IEdge<Node, Edge>
        {
            internal readonly T[,] index;
            internal readonly (int X, int Y) from;
            internal (sbyte X, sbyte Y) d;

            public Edge(T[,] index, (int X, int Y) from, (sbyte X, sbyte Y) d)
            {
                this.index = index;
                this.from = from;
                this.d = d;
            }

            /// <inheritdoc />
            public Node From => new Node(index, from);

            /// <inheritdoc />
            public Node To => new Node(index, (from.X + d.X, from.Y + d.Y));

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Edge e
                && Equals(index, e.index)
                && Equals(from, e.from)
                && Equals(d, e.d);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(
                index,
                from,
                d);
        }

        private struct TileAdjacencyCollection : IReadOnlyCollection<Edge>
        {
            internal readonly TileAdjacencyEnumerator enumeratorPrototype;

            internal TileAdjacencyCollection(T[,] index, (int X, int Y) coordinates)
            {
                enumeratorPrototype = new TileAdjacencyEnumerator(index, coordinates);
            }

            public int Count => this.Count();

            public IEnumerator<Edge> GetEnumerator() => enumeratorPrototype;

            IEnumerator IEnumerable.GetEnumerator() => enumeratorPrototype;
        }

        private struct TileAdjacencyEnumerator : IEnumerator<Edge>
        {
            internal Edge current;

            internal TileAdjacencyEnumerator(T[,] index, (int X, int Y) coordinates)
            {
                current = new Edge(index, coordinates, (-2, -1));
            }

            public Edge Current => current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                do
                {
                    current.d.X++;
                    if (current.d.X > 1)
                    {
                        current.d.X = -1;
                        current.d.Y++;
                        if (current.d.Y > 1)
                        {
                            return false;
                        }
                    }
                }
                while (
                    current.from.X + current.d.X < current.index.GetLowerBound(0)
                    || current.from.X + current.d.X > current.index.GetUpperBound(0)
                    || current.from.Y + current.d.Y < current.index.GetLowerBound(1)
                    || current.from.Y + current.d.Y > current.index.GetUpperBound(1)
                    || current.d == (0, 0));

                return true;
            }

            public void Reset() => current.d = (-2, -1);
        }
    }
}

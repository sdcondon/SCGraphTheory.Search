using SCGraphTheory.AdjacencyList;

namespace Search.Benchmarks.GraphImplementations
{
    public class RefSquareGridGraph
    {
        public static Graph<Node, Edge> Create((int X, int Y) size, out Node originNode)
        {
            var graph = new Graph<Node, Edge>();
            var nodeIndex = new Node[size.X, size.Y];

            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    graph.Add(nodeIndex[x, y] = new Node(x, y));
                }
            }

            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (nodeIndex.GetLowerBound(0) <= x + dx && nodeIndex.GetUpperBound(0) >= x + dx)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if ((dx != 0 || dy != 0) && nodeIndex.GetLowerBound(1) <= y + dy && nodeIndex.GetUpperBound(1) >= y + dy)
                                {
                                    graph.Add(new Edge(nodeIndex[x, y], nodeIndex[x + dx, y + dy]));
                                }
                            }
                        }
                    }
                }
            }

            originNode = nodeIndex[0, 0];
            return graph;
        }

        public class Edge : EdgeBase<Node, Edge>
        {
            public Edge(Node from, Node to)
                : base(from, to)
            {
            }
        }

        public class Node : NodeBase<Node, Edge>
        {
            public Node(int x, int y) => Coordinates = (x, y);

            public (int X, int Y) Coordinates { get; }
        }
    }
}

using SCGraphTheory.Search.Classic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WorldGraph = SCGraphTheory.Search.TestGraphs.GridGraph<SCGraphTheory.Search.Visualizer.World.Terrain>;

namespace SCGraphTheory.Search.Visualizer
{
    /// <summary>
    /// Search visualizer world model.
    /// </summary>
    public class World
    {
        private static readonly Dictionary<Terrain, float> TerrainCosts = new ()
        {
            [Terrain.Floor] = 1f,
            [Terrain.Water] = 3f,
        };

        private readonly Stopwatch searchTimer = new ();
        private readonly WorldGraph graph;

        private (int X, int Y) startPosition;
        private (int X, int Y) targetPosition;
        private Action recreateSearch;

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="size">The size of the world.</param>
        public World((int X, int Y) size)
        {
            Size = size;
            graph = new WorldGraph(size, (from, to) => from != Terrain.Wall && to != Terrain.Wall);

            Target = (0, 0);
            Start = (size.X - 1, size.Y - 1);

            RecreateSearch = MakeAStarSearch;
        }

        /// <summary>
        /// An enumeration of possible terrain types.
        /// </summary>
        public enum Terrain
        {
            /// <summary>
            /// Floor - cheap terrain type to travel on.
            /// </summary>
            Floor,

            /// <summary>
            /// Floor - expensive terrain type to travel on.
            /// </summary>
            Water,

            /// <summary>
            /// Wall - impassable terrain.
            /// </summary>
            Wall,
        }

        /// <summary>
        /// Gets the size of the world.
        /// </summary>
        public (int X, int Y) Size { get; }

        /// <summary>
        /// Gets the current search.
        /// </summary>
        public ISearch<WorldGraph.Node, WorldGraph.Edge> LatestSearch { get; private set; }

        /// <summary>
        /// Gets or sets the current start position.
        /// </summary>
        public (int X, int Y) Start
        {
            get => startPosition;
            set
            {
                startPosition = value;
                recreateSearch?.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets the current target position.
        /// </summary>
        public (int X, int Y) Target
        {
            get => targetPosition;
            set
            {
                targetPosition = value;
                recreateSearch?.Invoke();
            }
        }

        /// <summary>
        /// Gets the duration of the current search.
        /// </summary>
        public TimeSpan SearchDuration => searchTimer.Elapsed;

        /// <summary>
        /// Sets the delegate used to recreate the current search (and invokes it immediately). Invoked whenever some aspect of the world changes.
        /// </summary>
        public Action RecreateSearch
        {
            set
            {
                recreateSearch = value;
                recreateSearch?.Invoke();
            }
        }

        /// <summary>
        /// Gets an index of node values by position.
        /// </summary>
        /// <param name="x">The x-ordinate of the node to retrieve.</param>
        /// <param name="y">The y-ordinate of the node to retrieve.</param>
        public Terrain this[int x, int y]
        {
            get => graph[x, y].Value;
            set
            {
                if (x < 0 || x >= Size.X || y < 0 || y >= Size.Y)
                {
                    return;
                }

                graph[x, y].Value = value;
                recreateSearch?.Invoke();
            }
        }

        /// <summary>
        /// Recreates the active search using the A* algorithm.
        /// </summary>
        public void MakeAStarSearch()
        {
            searchTimer.Restart();
            LatestSearch = new AStarSearch<WorldGraph.Node, WorldGraph.Edge>(
                graph[Start.X, Start.Y],
                t => t.Coordinates == Target,
                EuclideanEdgeCost,
                n => ManhattanDistance(n.Coordinates, Target));
            LatestSearch.Complete();
            searchTimer.Stop();
        }

        /// <summary>
        /// Recreates the active search using Dijkstra's algorithm.
        /// </summary>
        public void MakeDijkstraSearch()
        {
            searchTimer.Restart();
            LatestSearch = new DijkstraSearch<WorldGraph.Node, WorldGraph.Edge>(
                graph[Start.X, Start.Y],
                t => t.Coordinates == Target,
                EuclideanEdgeCost);
            LatestSearch.Complete();
            searchTimer.Stop();
        }

        /// <summary>
        /// Recreates the active search using the breadth-first algorithm.
        /// </summary>
        public void MakeBreadthFirstSearch()
        {
            searchTimer.Restart();
            LatestSearch = new BreadthFirstSearch<WorldGraph.Node, WorldGraph.Edge>(
                graph[Start.X, Start.Y],
                t => t.Coordinates == Target);
            LatestSearch.Complete();
            searchTimer.Stop();
        }

        /// <summary>
        /// Recreates the active search using the depth-first algorithm.
        /// </summary>
        public void MakeDepthFirstSearch()
        {
            searchTimer.Restart();
            LatestSearch = new DepthFirstSearch<WorldGraph.Node, WorldGraph.Edge>(
                graph[Start.X, Start.Y],
                t => t.Coordinates == Target);
            LatestSearch.Complete();
            searchTimer.Stop();
        }

        private static float EuclideanDistance((int X, int Y) a, (int X, int Y) b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        private static float ManhattanDistance((int X, int Y) a, (int X, int Y) b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static float EuclideanEdgeCost(WorldGraph.Edge edge)
        {
            return EuclideanDistance(edge.From.Coordinates, edge.To.Coordinates) * 0.5f * (TerrainCosts[edge.From.Value] + TerrainCosts[edge.To.Value]);
        }
    }
}

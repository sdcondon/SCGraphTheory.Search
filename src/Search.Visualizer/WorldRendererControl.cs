using SCGraphTheory.Search.Classic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WorldGraph = SCGraphTheory.Search.TestGraphs.GridGraph<SCGraphTheory.Search.Visualizer.World.Terrain>;

namespace SCGraphTheory.Search.Visualizer
{
    /// <summary>
    /// Custom control class for rendering <see cref="World"/> instances.
    /// </summary>
    public class WorldRendererControl : Control
    {
        private readonly GdiWorldRenderer renderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorldRendererControl"/> class.
        /// </summary>
        /// <param name="world">The model to be rendered.</param>
        /// <param name="tileSize">The side length of each world tile, in pixels.</param>
        public WorldRendererControl(World world, int tileSize)
        {
            this.renderer = new GdiWorldRenderer(world, tileSize);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
        }

        /// <summary>
        /// Gets or sets the action to be invoked when a tile is clicked on.
        /// </summary>
        public Action<int, int> ClickHandler { get; set; }

        /// <inheritdoc />
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            var cellX = e.X / renderer.TileSize;
            var cellY = e.Y / renderer.TileSize;

            ClickHandler?.Invoke(cellX, cellY);
            this.Invalidate();
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs pe)
        {
            renderer.Render(pe.Graphics);
        }

        private class GdiWorldRenderer
        {
            private readonly Dictionary<World.Terrain, Brush> brushes;
            private readonly World world;

            public GdiWorldRenderer(World world, int tile)
            {
                this.world = world;
                this.TileSize = tile;

                brushes = new Dictionary<World.Terrain, Brush>()
                {
                    [World.Terrain.Floor] = Brushes.White,
                    [World.Terrain.Water] = Brushes.RoyalBlue,
                    [World.Terrain.Wall] = Brushes.Black,
                };
            }

            public int TileSize { get; set; }

            public void Render(Graphics gdi)
            {
                // Render terrain
                for (int x = 0; x < world.Size.X; x++)
                {
                    for (int y = 0; y < world.Size.Y; y++)
                    {
                        var rect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);
                        var brush = brushes[this.world[x, y]];
                        gdi.FillRectangle(brush, rect);
                    }
                }

                // Render visited edges
                foreach (var v in this.world.LatestSearch.Visited.Values)
                {
                    if (!Equals(v.Edge, default))
                    {
                        DrawGraphEdge(gdi, v.IsOnFrontier ? Pens.LightGray : Pens.Blue, v.Edge);
                    }
                }

                // Render path to target
                if (!Equals(this.world.LatestSearch.Target, default))
                {
                    foreach (var edge in this.world.LatestSearch.PathToTarget())
                    {
                        DrawGraphEdge(gdi, Pens.Red, edge);
                    }
                }

                // Render start & target
                DrawEllipse(gdi, Brushes.Green, world.Start.X, world.Start.Y, 0.75);
                DrawEllipse(gdi, Brushes.Red, world.Target.X, world.Target.Y, 0.75);

                gdi.DrawString($"Search duration: {this.world.SearchDuration}", SystemFonts.DefaultFont, Brushes.Black, new PointF(0, 0));
            }

            private void DrawGraphEdge(Graphics gdi, Pen pen, WorldGraph.Edge edge)
            {
                var from = CoordinateToPoint(edge.From.Coordinates.X, edge.From.Coordinates.Y);
                var to = CoordinateToPoint(edge.To.Coordinates.X, edge.To.Coordinates.Y);
                gdi.DrawLine(pen, from, to);
            }

            private void DrawEllipse(Graphics gdi, Brush brush, int x, int y, double size)
            {
                var rect = new Rectangle(CoordinateToPoint(x, y, -size / 2, -size / 2), new Size((int)(TileSize * size), (int)(TileSize * size)));
                gdi.FillEllipse(brush, rect);
            }

            private Point CoordinateToPoint(int x, int y, double xOffset = 0.0, double yOffset = 0.0)
            {
                return new Point((x * TileSize) + (int)(TileSize * (xOffset + 0.5)), (y * TileSize) + (int)(TileSize * (yOffset + 0.5)));
            }
        }
    }
}

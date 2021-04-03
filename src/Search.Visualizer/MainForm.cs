using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SCGraphTheory.Search.Visualizer
{
    public partial class MainForm : Form
    {
        private IDictionary<ToolStripButton, Action<int, int>> brushButtons;
        private IDictionary<ToolStripButton, Action> algorithmButtons;

        private WorldRendererControl gameControl;

        public MainForm()
        {
            InitializeComponent();

            int cellSize = 10;
            var world = new World((this.ClientSize.Width / cellSize, (this.ClientSize.Height - this.toolStrip1.Height) / cellSize));

            brushButtons = new Dictionary<ToolStripButton, Action<int, int>>
            {
                [paintStartButton] = (x, y) => world.Start = (x, y),
                [paintTargetButton] = (x, y) => world.Target = (x, y),
                [paintFloorButton] = (x, y) => world[x, y] = World.Terrain.Floor,
                [paintWaterButton] = (x, y) => world[x, y] = World.Terrain.Water,
                [paintWallButton] = (x, y) => world[x, y] = World.Terrain.Wall,
            };

            algorithmButtons = new Dictionary<ToolStripButton, Action>
            {
                [useBreadthFirstButton] = () => { world.RecreateSearch = world.MakeBreadthFirstSearch; },
                [useDepthFirstButton] = () => { world.RecreateSearch = world.MakeDepthFirstSearch; },
                [useDjikstraButton] = () => { world.RecreateSearch = world.MakeDijkstraSearch; },
                [useAStarButton] = () => { world.RecreateSearch = world.MakeAStarSearch; },
            };

            this.gameControl = new WorldRendererControl(world, cellSize);

            this.SuspendLayout();

            this.gameControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.gameControl.Location = new Point(0, this.toolStrip1.Bottom);
            this.gameControl.Name = "gameWorld";
            this.gameControl.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - this.toolStrip1.Height);
            this.gameControl.TabIndex = 0;
            this.gameControl.Parent = this;

            this.ResumeLayout(false);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private void BrushButton_Click(object sender, EventArgs e)
        {
            var pressedButton = sender as ToolStripButton;
            RadioToolStripButtonClick(pressedButton, brushButtons.Keys);
            gameControl.ClickHandler = brushButtons[pressedButton];
        }

        private void AlgorithmButton_Click(object sender, EventArgs e)
        {
            var pressedButton = sender as ToolStripButton;
            RadioToolStripButtonClick(pressedButton, algorithmButtons.Keys);
            algorithmButtons[pressedButton]();
        }

        private void RadioToolStripButtonClick(ToolStripButton clickedButton, IEnumerable<ToolStripButton> group)
        {
            if (!clickedButton.Checked)
            {
                foreach (var button in group)
                {
                    button.Checked = false;
                }

                clickedButton.Checked = true;
            }
        }
    }
}

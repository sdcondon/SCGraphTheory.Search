namespace SCGraphTheory.Search.Visualizer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.paintStartButton = new System.Windows.Forms.ToolStripButton();
            this.paintTargetButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.paintFloorButton = new System.Windows.Forms.ToolStripButton();
            this.paintWaterButton = new System.Windows.Forms.ToolStripButton();
            this.paintWallButton = new System.Windows.Forms.ToolStripButton();
            this.useAStarButton = new System.Windows.Forms.ToolStripButton();
            this.useDjikstraButton = new System.Windows.Forms.ToolStripButton();
            this.useDepthFirstButton = new System.Windows.Forms.ToolStripButton();
            this.useBreadthFirstButton = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.paintStartButton,
            this.paintTargetButton,
            this.toolStripSeparator1,
            this.paintFloorButton,
            this.paintWaterButton,
            this.paintWallButton,
            this.useAStarButton,
            this.useDjikstraButton,
            this.useDepthFirstButton,
            this.useBreadthFirstButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(486, 27);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // paintStartButton
            // 
            this.paintStartButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.paintStartButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.paintStartButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.paintStartButton.Name = "paintStartButton";
            this.paintStartButton.Size = new System.Drawing.Size(29, 24);
            this.paintStartButton.Text = "▶";
            this.paintStartButton.ToolTipText = "Paint start";
            this.paintStartButton.Click += new System.EventHandler(this.BrushButton_Click);
            // 
            // paintTargetButton
            // 
            this.paintTargetButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.paintTargetButton.ForeColor = System.Drawing.Color.Red;
            this.paintTargetButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.paintTargetButton.Name = "paintTargetButton";
            this.paintTargetButton.Size = new System.Drawing.Size(29, 24);
            this.paintTargetButton.Text = "●";
            this.paintTargetButton.ToolTipText = "Paint target";
            this.paintTargetButton.Click += new System.EventHandler(this.BrushButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // paintFloorButton
            // 
            this.paintFloorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.paintFloorButton.ForeColor = System.Drawing.Color.White;
            this.paintFloorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.paintFloorButton.Name = "paintFloorButton";
            this.paintFloorButton.Size = new System.Drawing.Size(29, 24);
            this.paintFloorButton.Text = "█";
            this.paintFloorButton.ToolTipText = "Paint floor";
            this.paintFloorButton.Click += new System.EventHandler(this.BrushButton_Click);
            // 
            // paintWaterButton
            // 
            this.paintWaterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.paintWaterButton.ForeColor = System.Drawing.Color.Blue;
            this.paintWaterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.paintWaterButton.Name = "paintWaterButton";
            this.paintWaterButton.Size = new System.Drawing.Size(29, 24);
            this.paintWaterButton.Text = "█";
            this.paintWaterButton.ToolTipText = "Paint water";
            this.paintWaterButton.Click += new System.EventHandler(this.BrushButton_Click);
            // 
            // paintWallButton
            // 
            this.paintWallButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.paintWallButton.ForeColor = System.Drawing.Color.Black;
            this.paintWallButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.paintWallButton.Name = "paintWallButton";
            this.paintWallButton.Size = new System.Drawing.Size(29, 24);
            this.paintWallButton.Text = "█";
            this.paintWallButton.ToolTipText = "Paint wall";
            this.paintWallButton.Click += new System.EventHandler(this.BrushButton_Click);
            // 
            // useAStarButton
            // 
            this.useAStarButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.useAStarButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.useAStarButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.useAStarButton.Name = "useAStarButton";
            this.useAStarButton.Size = new System.Drawing.Size(29, 24);
            this.useAStarButton.Text = "A*";
            this.useAStarButton.ToolTipText = "Use A* search";
            this.useAStarButton.Click += new System.EventHandler(this.AlgorithmButton_Click);
            // 
            // useDjikstraButton
            // 
            this.useDjikstraButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.useDjikstraButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.useDjikstraButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.useDjikstraButton.Name = "useDjikstraButton";
            this.useDjikstraButton.Size = new System.Drawing.Size(29, 24);
            this.useDjikstraButton.Text = "Dj";
            this.useDjikstraButton.ToolTipText = "Use Djikstra search";
            this.useDjikstraButton.Click += new System.EventHandler(this.AlgorithmButton_Click);
            // 
            // useDepthFirstButton
            // 
            this.useDepthFirstButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.useDepthFirstButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.useDepthFirstButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.useDepthFirstButton.Name = "useDepthFirstButton";
            this.useDepthFirstButton.Size = new System.Drawing.Size(31, 24);
            this.useDepthFirstButton.Text = "DF";
            this.useDepthFirstButton.ToolTipText = "Use depth first search";
            this.useDepthFirstButton.Click += new System.EventHandler(this.AlgorithmButton_Click);
            // 
            // useBreadthFirstButton
            // 
            this.useBreadthFirstButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.useBreadthFirstButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.useBreadthFirstButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.useBreadthFirstButton.Name = "useBreadthFirstButton";
            this.useBreadthFirstButton.Size = new System.Drawing.Size(29, 24);
            this.useBreadthFirstButton.Text = "BF";
            this.useBreadthFirstButton.ToolTipText = "Use breadth first search";
            this.useBreadthFirstButton.Click += new System.EventHandler(this.AlgorithmButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 542);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.Text = "Search Visualizer";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton paintStartButton;
        private System.Windows.Forms.ToolStripButton paintTargetButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton paintFloorButton;
        private System.Windows.Forms.ToolStripButton paintWaterButton;
        private System.Windows.Forms.ToolStripButton paintWallButton;
        private System.Windows.Forms.ToolStripButton useAStarButton;
        private System.Windows.Forms.ToolStripButton useDjikstraButton;
        private System.Windows.Forms.ToolStripButton useDepthFirstButton;
        private System.Windows.Forms.ToolStripButton useBreadthFirstButton;
    }
}


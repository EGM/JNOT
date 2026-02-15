using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Jnot.Shared.UI.Controls
{
    partial class ExpandableCard
    {
        private System.ComponentModel.IContainer components = null;

        private Panel headerPanel;
        private Label lblTitle;
        private PictureBox chevron;
        private Panel contentHost;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            headerPanel = new Panel();
            lblTitle = new Label();
            chevron = new PictureBox();
            contentHost = new Panel();

            SuspendLayout();

            // HEADER PANEL
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 32;
            headerPanel.Cursor = Cursors.Hand;
            headerPanel.Paint += headerPanel_Paint;
            headerPanel.MouseEnter += Header_MouseEnter;
            headerPanel.MouseLeave += Header_MouseLeave;
            headerPanel.MouseDown += Header_MouseDown;
            headerPanel.MouseUp += Header_MouseUp;

            // TITLE LABEL
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTitle.Padding = new Padding(12, 0, 0, 0);

            // CHEVRON
            chevron.Dock = DockStyle.Right;
            chevron.Width = 32;
            chevron.SizeMode = PictureBoxSizeMode.CenterImage;
            chevron.Cursor = Cursors.Hand;
            chevron.Click += (s, e) => ToggleExpanded();

            // CONTENT HOST
            contentHost.Dock = DockStyle.Top;
            contentHost.AutoSize = true;
            contentHost.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            contentHost.Padding = new Padding(10);

            // ASSEMBLE HEADER
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(chevron);

            // ASSEMBLE CONTROL
            Controls.Add(contentHost);
            Controls.Add(headerPanel);

            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.Transparent;
            Name = "ExpandableCard";
            Size = new Size(300, 100);

            ResumeLayout(false);
            PerformLayout();
        }
    }
}

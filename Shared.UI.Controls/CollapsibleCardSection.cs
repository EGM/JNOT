using System;
using System.Drawing;
using System.Windows.Forms;

namespace Jnot.Shared.UI.Controls
{
    public class CollapsibleCardSection : Panel
    {
        private readonly Panel _header;
        private readonly Panel _body;
        private readonly Label _titleLabel;
        private readonly PictureBox _chevron;

        private bool _expanded = true;

        public CollapsibleCardSection(string title)
        {
            DoubleBuffered = true;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Margin = new Padding(0, 6, 0, 6);

            // Header
            _header = new Panel
            {
                Height = 36,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10, 8, 10, 8),
                Cursor = Cursors.Hand
            };

            _header.MouseEnter += (s, e) => _header.BackColor = Color.FromArgb(235, 235, 235);
            _header.MouseLeave += (s, e) => _header.BackColor = Color.FromArgb(245, 245, 245);
            _header.Click += (s, e) => Toggle();

            // Chevron
            _chevron = new PictureBox
            {
                Image = Chevron(true),
                SizeMode = PictureBoxSizeMode.AutoSize,
                Dock = DockStyle.Left,
                Margin = new Padding(0, 0, 8, 0)
            };
            _chevron.Click += (s, e) => Toggle();

            // Title
            _titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 10),
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            _titleLabel.Click += (s, e) => Toggle();

            _header.Controls.Add(_titleLabel);
            _header.Controls.Add(_chevron);
            Controls.Add(_header);

            // Body
            _body = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12),
                MinimumSize = new Size(0, 20)   // <-- THE FIX
            };

            Controls.Add(_body);
        }

        public void Add(Control c)
        {
            _body.Controls.Add(c);
        }

        private void Toggle()
        {
            _expanded = !_expanded;
            _chevron.Image = Chevron(_expanded);
            _body.Visible = _expanded;
        }

        private Bitmap Chevron(bool expanded)
        {
            var bmp = new Bitmap(10, 10);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using var pen = new Pen(Color.Black, 2);

            if (expanded)
            {
                g.DrawLines(pen, new[]
                {
                    new Point(1, 3),
                    new Point(5, 7),
                    new Point(9, 3)
                });
            }
            else
            {
                g.DrawLines(pen, new[]
                {
                    new Point(3, 1),
                    new Point(7, 5),
                    new Point(3, 9)
                });
            }

            return bmp;
        }
    }
}

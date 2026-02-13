using System;
using System.Drawing;
using System.Windows.Forms;

namespace JNOT.Shared.UI.Controls
{
    public class CardSection : Panel
    {
        private readonly Panel _header;
        private readonly Panel _body;
        private readonly Label _titleLabel;

        public CardSection(string title)
        {
            DoubleBuffered = true;

            // Excel-safe container settings
            AutoSize = false;                 // Excel ignores AutoSize anyway
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Margin = new Padding(0, 6, 0, 6);
            Padding = new Padding(0);

            // -------------------------
            // Header (always visible)
            // -------------------------
            _header = new Panel
            {
                Height = 36,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10, 8, 10, 8)
            };

            _titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 10),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _header.Controls.Add(_titleLabel);
            Controls.Add(_header);

            // -------------------------
            // Body (Excel-safe)
            // -------------------------
            _body = new Panel
            {
                Dock = DockStyle.Top,          // stack sections vertically
                AutoSize = false,              // Excel cannot handle AutoSize here
                Height = 40,                   // guaranteed visible
                Padding = new Padding(12),
                BackColor = Color.White
            };

            Controls.Add(_body);
        }

        public void Add(Control c)
        {
            // Add children normally
            c.Dock = DockStyle.Top;            // stack children vertically
            _body.Controls.Add(c);

            // Ensure body grows as children are added
            _body.Height = Math.Max(_body.Height, c.Height + 12);
        }
    }
}
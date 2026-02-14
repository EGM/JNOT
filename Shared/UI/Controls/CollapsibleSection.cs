using System;
using System.Drawing;
using System.Windows.Forms;

namespace JNOT.Shared.UI.Controls
{
    public class CollapsibleSection : Panel
    {
        private readonly Panel _content;
        private readonly Button _toggle;
        private bool _expanded = true;

        public CollapsibleSection(string title)
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            _toggle = new Button
            {
                Text = "▼ " + title,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            _toggle.Click += (s, e) => Toggle();

            _content = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(20, 5, 0, 5)
            };

            Controls.Add(_toggle);
            Controls.Add(_content);
        }

        public void Add(Control control)
        {
            _content.Controls.Add(control);
        }

        private void Toggle()
        {
            _expanded = !_expanded;
            _content.Visible = _expanded;
            _toggle.Text = (_expanded ? "▼ " : "► ") + _toggle.Text.Substring(2);
        }
    }
}

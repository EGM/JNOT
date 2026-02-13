
## 📁 Directory: /


## 📁 Directory: Controls

- Controls\CollapsibleCardSection.cs

```cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JNOT.Shared.UI.Controls
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
                Padding = new Padding(12)
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
```

- Controls\CollapsibleSection.cs

```cs
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
```

- Controls\JnotLogLevel.cs

```cs
namespace JNOT.Shared.UI.Controls
{
    public enum JnotLogLevel
    {
        Debug = 0,
        Info = 1,
        Success = 2,
        Warning = 3,
        Error = 4
    }
}
```

- Controls\JnotRichOutputBox .cs

```cs
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JNOT.Shared.UI.Controls
{
    public class JnotRichOutputBox : RichTextBox
    {
        // Filtering:
        // - Error & Success always show
        // - Debug/Info/Warning respect MinVisibleLevel
        public JnotLogLevel MinVisibleLevel { get; set; } = JnotLogLevel.Info;

        public bool ShowTimestamps { get; set; } = true;

        public Color InfoColor { get; set; } = Color.Black;
        public Color SuccessColor { get; set; } = Color.Green;       // 💚
        public Color WarningColor { get; set; } = Color.DarkOrange;
        public Color ErrorColor { get; set; } = Color.DarkRed;
        public Color DebugColor { get; set; } = Color.DimGray;

        public JnotRichOutputBox()
        {
            ReadOnly = true;
            DetectUrls = true;
            HideSelection = false;
            BorderStyle = BorderStyle.FixedSingle;
            ContextMenuStrip = BuildContextMenu();
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            var copyItem = new ToolStripMenuItem("Copy");
            copyItem.Click += (s, e) => Copy();

            var copyAllItem = new ToolStripMenuItem("Copy All");
            copyAllItem.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(Text))
                    Clipboard.SetText(Text);
            };

            var copyRtfItem = new ToolStripMenuItem("Copy as RTF");
            copyRtfItem.Click += (s, e) =>
            {
                if (SelectionLength > 0)
                    Clipboard.SetText(SelectedRtf, TextDataFormat.Rtf);
                else if (!string.IsNullOrEmpty(Rtf))
                    Clipboard.SetText(Rtf, TextDataFormat.Rtf);
            };

            var clearItem = new ToolStripMenuItem("Clear");
            clearItem.Click += (s, e) => Clear();

            menu.Items.Add(copyItem);
            menu.Items.Add(copyAllItem);
            menu.Items.Add(copyRtfItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(clearItem);

            return menu;
        }

        // ------------------------------------------------------------
        // TEXTBOX-LIKE API (AppendText, AppendLine)
        // ------------------------------------------------------------

        public new void AppendText(string text)
        {
            AppendLine(text, JnotLogLevel.Info);
        }

        public void AppendLine(string text)
        {
            AppendLine(text, JnotLogLevel.Info);
        }

        // ------------------------------------------------------------
        // RICH LOGGING API
        // ------------------------------------------------------------

        public void AppendInfo(string message) =>
            AppendLine(message, JnotLogLevel.Info);

        public void AppendSuccess(string message) =>
            AppendLine(message, JnotLogLevel.Success);

        public void AppendWarning(string message) =>
            AppendLine(message, JnotLogLevel.Warning);

        public void AppendError(string message) =>
            AppendLine(message, JnotLogLevel.Error);

        public void AppendDebug(string message) =>
            AppendLine(message, JnotLogLevel.Debug);

        public void AppendLine(string message, JnotLogLevel level)
        {
            if (!ShouldDisplay(level))
                return;

            var line = BuildLine(message, level);
            var color = GetColor(level);

            AppendColoredLine(line, color);
        }

        private bool ShouldDisplay(JnotLogLevel level)
        {
            // Errors and Success always show
            if (level == JnotLogLevel.Error || level == JnotLogLevel.Success)
                return true;

            // Others respect MinVisibleLevel
            return level >= MinVisibleLevel;
        }

        private string BuildLine(string message, JnotLogLevel level)
        {
            var sb = new StringBuilder();

            if (ShowTimestamps)
            {
                sb.Append(DateTime.Now.ToString("HH:mm:ss"));
                sb.Append(" ");
            }

            sb.Append("[");
            sb.Append(level.ToString().ToUpperInvariant());
            sb.Append("] ");
            sb.Append(message);

            if (!message.EndsWith(Environment.NewLine))
                sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        private Color GetColor(JnotLogLevel level)
        {
            switch (level)
            {
                case JnotLogLevel.Info: return InfoColor;
                case JnotLogLevel.Success: return SuccessColor;
                case JnotLogLevel.Warning: return WarningColor;
                case JnotLogLevel.Error: return ErrorColor;
                case JnotLogLevel.Debug: return DebugColor;
                default: return InfoColor;
            }
        }

        private void AppendColoredLine(string text, Color color)
        {
            var oldReadOnly = ReadOnly;
            ReadOnly = false;

            SelectionStart = TextLength;
            SelectionLength = 0;

            SelectionColor = color;
            SelectedText = text;
            SelectionColor = ForeColor;

            ReadOnly = oldReadOnly;
            ScrollToCaret();
        }
    }
}
```

- ITaskPaneContentProvider.cs

```cs
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JNOT.Shared.UI
{
    public interface ITaskPaneContentProvider
    {
        string Title { get; }
        string Status { get; }

        /// <summary>
        /// Populate the pane asynchronously.
        /// </summary>
        Task PopulateAsync(Panel contentPanel);
    }
}
```

- JnotTaskPane.cs

```cs
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JNOT.Shared.UI
{
    public class JnotTaskPane : UserControl
    {
        private readonly Panel _contentPanel;
        private readonly Label _titleLabel;
        private readonly Label _statusLabel;

        public JnotTaskPane()
        {
            Dock = DockStyle.Fill;

            _titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                Padding = new Padding(4, 4, 4, 0)
            };

            _statusLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 18,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Font = new System.Drawing.Font("Segoe UI", 8),
                Padding = new Padding(4, 0, 4, 4)
            };

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            Controls.Add(_contentPanel);
            Controls.Add(_statusLabel);
            Controls.Add(_titleLabel);
        }

        public async Task LoadFromAsync(ITaskPaneContentProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _titleLabel.Text = provider.Title;
            _statusLabel.Text = provider.Status;

            _contentPanel.Controls.Clear();

            // Let the provider populate the pane asynchronously
            await provider.PopulateAsync(_contentPanel);
        }
    }
}
```


## 📁 Directory: Panels

- Panels\OutputPanel.cs

```cs
using System.Windows.Forms;
using JNOT.Shared.UI.Controls;

namespace JNOT.Shared.UI.Panels
{
    public class OutputPanel : UserControl
    {
        private readonly JnotRichOutputBox _output = new JnotRichOutputBox();

        public bool ClearOnRun { get; set; } = true;

        public JnotRichOutputBox Output => _output;

        public OutputPanel()
        {
            _output.Dock = DockStyle.Fill;
            Controls.Add(_output);
        }

        public void PrepareForRun()
        {
            if (ClearOnRun)
                _output.Clear();
        }
    }
}
```


## 📁 Directory: Properties

- Properties\AssemblyInfo.cs

```cs
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("UI")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("UI")]
[assembly: AssemblyCopyright("Copyright ©  2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("87bebfd8-e577-4b87-aef6-d9986fa43509")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

```

- Shared.UI.csproj
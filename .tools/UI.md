
## 📁 Directory: /


## 📁 Directory: Controls

- Controls\ExpandableCard.cs

```cs
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace JNOT.Shared.UI.Controls
{
    public partial class ExpandableCard : UserControl
    {
        private const int CornerRadius = 3;
        private const int AnimationDuration = 150;

        private bool _isExpanded = true;
        private bool _hover;
        private bool _pressed;

        private Timer _animationTimer;
        private int _animationStartHeight;
        private int _animationTargetHeight;
        private DateTime _animationStartTime;

        private Control _content;

        public event EventHandler<bool> ExpandedChanged;

        public ExpandableCard()
        {
            InitializeComponent();
            DoubleBuffered = true;
            UpdateChevron();
        }

        // ---------------------------
        // PUBLIC PROPERTIES
        // ---------------------------

        [Category("Appearance")]
        public string HeaderText
        {
            get => lblTitle.Text;
            set => lblTitle.Text = value;
        }

        [Category("Behavior")]
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                AnimateExpansion();
                UpdateChevron();
                ExpandedChanged?.Invoke(this, _isExpanded);
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Control Content
        {
            get => _content;
            set
            {
                if (_content != null)
                    contentHost.Controls.Remove(_content);

                _content = value;

                if (_content != null)
                {
                    _content.Dock = DockStyle.Top;
                    contentHost.Controls.Add(_content);
                }

                if (_isExpanded)
                    contentHost.Height = GetContentHeight();
            }
        }

        // ---------------------------
        // HEADER INTERACTION
        // ---------------------------

        private void Header_MouseEnter(object sender, EventArgs e)
        {
            _hover = true;
            headerPanel.Invalidate();
        }

        private void Header_MouseLeave(object sender, EventArgs e)
        {
            _hover = false;
            _pressed = false;
            headerPanel.Invalidate();
        }

        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            _pressed = true;
            headerPanel.Invalidate();
        }

        private void Header_MouseUp(object sender, MouseEventArgs e)
        {
            _pressed = false;
            headerPanel.Invalidate();
            ToggleExpanded();
        }

        private void ToggleExpanded() => IsExpanded = !IsExpanded;

        private void UpdateChevron()
        {
            chevron.Image = _isExpanded
                ? Properties.Resources.ChevronUp32
                : Properties.Resources.ChevronDown32;
        }

        // ---------------------------
        // HEADER PAINTING
        // ---------------------------

        private void headerPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, headerPanel.Width - 1, headerPanel.Height - 1);

            // Background gradient
            Color top, bottom;

            if (_pressed)
            {
                top = bottom = Color.FromArgb(224, 224, 224);
            }
            else if (_hover)
            {
                top = Color.FromArgb(240, 240, 240);
                bottom = Color.FromArgb(230, 230, 230);
            }
            else
            {
                top = Color.FromArgb(248, 248, 248);
                bottom = Color.FromArgb(237, 237, 237);
            }

            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, top, bottom, 90f))
                g.FillRectangle(brush, rect);

            // Border
            using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                g.DrawRectangle(pen, rect);

            // Rounded corners
            using (var path = RoundedRect(rect, CornerRadius))
            using (var region = new Region(path))
                headerPanel.Region = region;
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ---------------------------
        // EXPANSION ANIMATION
        // ---------------------------

        private void AnimateExpansion()
        {
            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer.Dispose();
            }

            _animationStartHeight = contentHost.Height;
            _animationTargetHeight = _isExpanded ? GetContentHeight() : 0;
            _animationStartTime = DateTime.Now;

            _animationTimer = new Timer { Interval = 15 };
            _animationTimer.Tick += AnimationTick;
            _animationTimer.Start();
        }

        private int GetContentHeight()
        {
            if (_content == null) return 0;
            return _content.PreferredSize.Height + contentHost.Padding.Vertical;
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            double elapsed = (DateTime.Now - _animationStartTime).TotalMilliseconds;
            double progress = Math.Min(1.0, elapsed / AnimationDuration);

            int newHeight = (int)(_animationStartHeight +
                                  ((_animationTargetHeight - _animationStartHeight) * progress));

            contentHost.Height = newHeight;

            if (progress >= 1.0)
            {
                _animationTimer.Stop();
                _animationTimer.Dispose();
            }
        }
    }
}
```

- Controls\ExpandableCard.Designer.cs

```cs
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;

namespace JNOT.Shared.UI.Controls
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
            lblTitle.Font = new Font("Segoe UI", 9F, FontStyle.Semibold);
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

```

- Controls\ExpandableSection.cs

```cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JNOT.Shared.UI.Controls
{
    public partial class ExpandableSection : UserControl
    {
        public ExpandableSection()
        {
            InitializeComponent();
        }
    }
}

```

- Controls\ExpandableSection.Designer.cs

```cs
namespace JNOT.Shared.UI.Controls
{
    partial class ExpandableSection
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        #endregion
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

- packages.config

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


## 📁 Directory: resources

- resources\down-chevron.png
- resources\up-chevron.png
- Shared.UI.csproj
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
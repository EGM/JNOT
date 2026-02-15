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
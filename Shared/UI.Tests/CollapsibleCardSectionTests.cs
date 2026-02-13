using System;
using System.Drawing;
using System.Windows.Forms;
using JNOT.Shared.UI.Controls;
using System.Linq;

namespace Shared.UI.Tests
{
    public class CollapsibleCardSectionTests
    {
        // ------------------------------------------------------------
        // CREATION
        // ------------------------------------------------------------

        [Fact]
        public void Constructor_SetsUpHeaderAndBody()
        {
            var section = new CollapsibleCardSection("Test Title");

            Assert.Equal(Color.White, section.BackColor);
            Assert.Equal(BorderStyle.FixedSingle, section.BorderStyle);

            // Order: header added first, then body
            Assert.IsType<Panel>(section.Controls[0]); // header
            Assert.IsType<Panel>(section.Controls[1]); // body
        }

        [Fact]
        public void Constructor_SetsTitleCorrectly()
        {
            var section = new CollapsibleCardSection("Hello World");

            var header = (Panel)section.Controls[0];
            var titleLabel = (Label)header.Controls[0];

            Assert.Equal("Hello World", titleLabel.Text);
        }

        // ------------------------------------------------------------
        // POPULATION
        // ------------------------------------------------------------

        [Fact]
        public void Add_AddsControlToBody()
        {
            var section = new CollapsibleCardSection("Test");
            var button = new Button();

            section.Add(button);

            var body = (Panel)section.Controls[1];

            Assert.Single(body.Controls);
            Assert.Same(button, body.Controls[0]);
        }

        // ------------------------------------------------------------
        // TOGGLING
        // ------------------------------------------------------------

        [Fact]
        public void Toggle_HidesBody_WhenExpanded()
        {
            var section = new CollapsibleCardSection("Test");
            var body = (Panel)section.Controls[1];

            Assert.True(body.Visible);

            InvokeToggle(section);

            Assert.False(body.Visible);
        }

        [Fact]
        public void Toggle_ShowsBody_WhenCollapsed()
        {
            var section = new CollapsibleCardSection("Test");
            var body = (Panel)section.Controls[1];

            // Collapse
            InvokeToggle(section);
            Assert.False(body.Visible);

            // Expand
            InvokeToggle(section);
            Assert.True(body.Visible);
        }

        [Fact]
        public void Toggle_UpdatesChevronImage()
        {
            var section = new CollapsibleCardSection("Test");

            var header = (Panel)section.Controls[0];
            var chevron = (PictureBox)header.Controls[1];

            var initial = chevron.Image;

            InvokeToggle(section);
            var afterToggle = chevron.Image;

            Assert.NotSame(initial, afterToggle);
        }

        // ------------------------------------------------------------
        // DESTRUCTION
        // ------------------------------------------------------------

        [Fact]
        public void Dispose_DisposesChildControls()
        {
            var section = new CollapsibleCardSection("Test");
            var body = (Panel)section.Controls[1];
            var header = (Panel)section.Controls[0];

            section.Dispose();

            Assert.True(section.IsDisposed);
            Assert.True(body.IsDisposed);
            Assert.True(header.IsDisposed);
        }

        // ------------------------------------------------------------
        // HELPERS
        // ------------------------------------------------------------

        private static void InvokeToggle(CollapsibleCardSection section)
        {
            // Click the header (which is wired to Toggle)
            var header = section.Controls[0];
            header.GetType()
                  .GetMethod("OnClick",
                      System.Reflection.BindingFlags.Instance |
                      System.Reflection.BindingFlags.NonPublic)
                  ?.Invoke(header, new object[] { EventArgs.Empty });
        }

        // More Tests
        [Fact]
        public void Constructor_InitializesExpandedState()
        {
            var section = new CollapsibleCardSection("Test");

            // Body should be visible when expanded
            var body = (Panel)section.Controls[1];
            Assert.True(body.Visible);
        }

        [Fact]
        public void Constructor_SetsTitle()
        {
            var section = new CollapsibleCardSection("Hello");

            var header = (Panel)section.Controls[0];
            var title = (Label)header.Controls[0];

            Assert.Equal("Hello", title.Text);
        }

        [Fact]
        public void Constructor_SetsDefaultStyling()
        {
            var section = new CollapsibleCardSection("Test");

            Assert.Equal(Color.White, section.BackColor);
            Assert.Equal(BorderStyle.FixedSingle, section.BorderStyle);
            Assert.True(section.AutoSize);
        }

        [Fact]
        public void Add_AddsControlToBody_AndOnlyBody()
        {
            var section = new CollapsibleCardSection("Test");
            var button = new Button();

            section.Add(button);

            var header = (Panel)section.Controls[0];
            var body = (Panel)section.Controls[1];

            Assert.Empty(
    header.Controls
          .Cast<Control>()
          .Where(c => c is Button)
);
            Assert.Single(body.Controls);
            Assert.Same(button, body.Controls[0]);
        }

        [Fact]
        public void Add_MultipleControls_AppearInOrder()
        {
            var section = new CollapsibleCardSection("Test");

            var a = new Label();
            var b = new TextBox();
            var c = new Button();

            section.Add(a);
            section.Add(b);
            section.Add(c);

            var body = (Panel)section.Controls[1];

            Assert.Equal(3, body.Controls.Count);
            Assert.Same(a, body.Controls[0]);
            Assert.Same(b, body.Controls[1]);
            Assert.Same(c, body.Controls[2]);
        }

        [Fact]
        public void Add_ExpandsBodyHeight()
        {
            var section = new CollapsibleCardSection("Test");
            var body = (Panel)section.Controls[1];

            var initialHeight = body.Height;

            section.Add(new Label { Height = 20 });

            Assert.True(body.Height > initialHeight);
        }

        [Fact]
        public void Control_CanBeCreated_ThenPopulated_WithoutErrors()
        {
            var section = new CollapsibleCardSection("Test");

            // Creation sanity check
            var header = (Panel)section.Controls[0];
            var body = (Panel)section.Controls[1];
            Assert.True(body.Visible);

            // Populate
            var child = new Label { Text = "Hello" };
            section.Add(child);

            // Validate
            Assert.Single(body.Controls);
            Assert.Same(child, body.Controls[0]);
        }

        [Fact]
        public void AddControlWhileCollapsed_DoesNotExpandBody()
        {
            var section = new CollapsibleCardSection("Test");
            var body = (Panel)section.Controls[1];

            // Collapse
            InvokeToggle(section);
            Assert.False(body.Visible);

            // Add control while collapsed
            section.Add(new Label());

            // Body should remain collapsed
            Assert.False(body.Visible);
        }

        [Fact]
        public void AddControlWhileCollapsed_AppearsWhenExpanded()
        {
            var section = new CollapsibleCardSection("Test");
            var body = (Panel)section.Controls[1];

            // Collapse
            InvokeToggle(section);

            var child = new Label();
            section.Add(child);

            // Expand
            InvokeToggle(section);

            Assert.Single(body.Controls);
            Assert.Same(child, body.Controls[0]);
            Assert.True(body.Visible);
        }

        [Fact]
        public void AddMultipleControlsWhileCollapsed_PreservesOrder()
        {
            var section = new CollapsibleCardSection("Test");
            var body = (Panel)section.Controls[1];

            InvokeToggle(section); // collapse

            var a = new Label();
            var b = new TextBox();
            var c = new Button();

            section.Add(a);
            section.Add(b);
            section.Add(c);

            InvokeToggle(section); // expand

            Assert.Equal(3, body.Controls.Count);
            Assert.Same(a, body.Controls[0]);
            Assert.Same(b, body.Controls[1]);
            Assert.Same(c, body.Controls[2]);
        }

        [Fact]
        public void RapidToggle_AlwaysEndsInCorrectState()
        {
            var section = new CollapsibleCardSection("Test");
            var body = (Panel)section.Controls[1];

            // Toggle 5 times
            for (int i = 0; i < 5; i++)
                InvokeToggle(section);

            // Odd number of toggles → collapsed
            Assert.False(body.Visible);

            // Toggle once more → expanded
            InvokeToggle(section);
            Assert.True(body.Visible);
        }

        [Fact]
        public void RapidToggle_UpdatesChevronEachTime()
        {
            var section = new CollapsibleCardSection("Test");
            var header = (Panel)section.Controls[0];
            var chevron = (PictureBox)header.Controls[1];

            var previous = chevron.Image;

            for (int i = 0; i < 3; i++)
            {
                InvokeToggle(section);
                Assert.NotSame(previous, chevron.Image);
                previous = chevron.Image;
            }
        }

        [Fact]
        public void MixedPopulationAndToggle_PreservesOrderAndState()
        {
            var section = new CollapsibleCardSection("Test");
            var body = (Panel)section.Controls[1];

            var a = new Label();
            section.Add(a);

            InvokeToggle(section); // collapse

            var b = new TextBox();
            section.Add(b);

            InvokeToggle(section); // expand

            var c = new Button();
            section.Add(c);

            Assert.Equal(3, body.Controls.Count);
            Assert.Same(a, body.Controls[0]);
            Assert.Same(b, body.Controls[1]);
            Assert.Same(c, body.Controls[2]);
            Assert.True(body.Visible);
        }

        [Fact]
        public void MixedPopulationAndToggle_UpdatesLayout()
        {
            var section = new CollapsibleCardSection("Test");
            var body = (Panel)section.Controls[1];

            var initialHeight = body.Height;

            section.Add(new Label { Height = 20 });
            InvokeToggle(section); // collapse
            InvokeToggle(section); // expand

            Assert.True(body.Height > initialHeight);
        }
    }
}
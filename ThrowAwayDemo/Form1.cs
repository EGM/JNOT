using JNOT.Shared.UI.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ThrowAwayDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            BuildDemo();
        }

        private void BuildDemo()
        {
            // Make the form friendlier
            Text = "ExpandableCard Demo";
            AutoScroll = true;

            // Create the card
            var card = new ExpandableCard
            {
                HeaderText = "Demo Section",
                Dock = DockStyle.Top,
                Margin = new Padding(10)
            };

            // Create simple content
            var contentPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            contentPanel.Controls.Add(new Label
            {
                Text = "Hello from inside the ExpandableCard!",
                AutoSize = true
            });

            contentPanel.Controls.Add(new Button
            {
                Text = "Click Me",
                AutoSize = true
            });

            // Attach content to the card
            card.Content = contentPanel;

            // Add card to the form
            Controls.Add(card);
        }
    }
}
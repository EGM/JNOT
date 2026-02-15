
## 📁 Directory: /

- Class1.cs

```cs
namespace Shared.UI
{
    public class Class1
    {
    }
}

```

- ITaskPaneContentProvider.cs

```cs
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jnot.Shared.UI
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

namespace Jnot.Shared.UI
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

- JnotTaskPane.resx
- Shared.UI.csproj
- Shared.UI.csproj.user
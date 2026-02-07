
## 📁 Directory: /

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
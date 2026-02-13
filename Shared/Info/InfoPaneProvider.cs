using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using JNOT.Shared.UI.Controls;

namespace JNOT.Shared.Info
{
    public class InfoPaneProvider
    {
        public string ExcelVersion { get; set; }
        public string VstoRuntimeVersion { get; set; }
        public string BuildDate { get; set; }
        public string Version { get; set; }

        public async Task PopulateAsync(Control host)
        {
            try
            {
                host.Controls.Clear();

                var root = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoScroll = true,
                    Padding = new Padding(12),
                };

                host.Controls.Add(root);

                // Title
                root.Controls.Add(new Label
                {
                    Text = "JNOT Toolbox for Excel",
                    Font = new Font("Segoe UI Semibold", 14),
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 12)
                });

                // Version Section
                var version = new CardSection("Version");
                version.Add(DiagnosticRow("Version", Version));
                version.Add(DiagnosticRow("Build Date", BuildDate));
                root.Controls.Add(version);

                // Components
                var components = new CardSection("Components");
                components.Add(Label("Shared.Config"));
                components.Add(Label("Shared.Info"));
                components.Add(Label("Shared.UI"));
                components.Add(Label("FileRenamer"));
                components.Add(Label("AddIn"));
                root.Controls.Add(components);

                // Paths
                var paths = new CardSection("Paths");
                paths.Add(Label($"Config: {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\JNOT"));
                paths.Add(Label($"Logs: {Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\JNOT"));
                root.Controls.Add(paths);

                // Diagnostics
                var diag = new CardSection("Diagnostics");
                diag.Add(DiagnosticRow("Excel Version", ExcelVersion));
                diag.Add(DiagnosticRow("VSTO Runtime", VstoRuntimeVersion));
                diag.Add(DiagnosticRow("OS Version", Environment.OSVersion.ToString()));
                diag.Add(DiagnosticRow(".NET Runtime", Environment.Version.ToString()));
                root.Controls.Add(diag);

                // Attributions
                var attrib = new CardSection("Attributions");
                attrib.Add(Label("Parser — MIT License"));
                attrib.Add(Label("Icons — Fluent UI System Icons"));
                root.Controls.Add(attrib);

                // Links
                var links = new CardSection("Links");
                links.Add(Link("Documentation", "https://egm.github.io/JNOT/"));
                links.Add(Link("GitHub Repo", "https://github.com/egm/JNOT"));
                root.Controls.Add(links);

                await Task.CompletedTask;
                System.Diagnostics.Debug.WriteLine("InfoPaneProvider: Info pane populated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("PopulateAsync FAILED:\n\n" + ex.ToString());
                System.Diagnostics.Debug.WriteLine("InfoPaneProvider blew up. :(");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        // -------------------------
        // UI Helpers
        // -------------------------

        private Control Label(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Margin = new Padding(4, 2, 4, 2)
            };
        }

        private Control Link(string text, string url)
        {
            var link = new LinkLabel
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Margin = new Padding(4, 2, 4, 2)
            };

            link.LinkClicked += (s, e) =>
            {
                try { System.Diagnostics.Process.Start(url); }
                catch { }
            };

            return link;
        }

        private Control DiagnosticRow(string label, string value)
        {
            var panel = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 2)
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            panel.Controls.Add(Label(label + ":"), 0, 0);
            panel.Controls.Add(Label(value), 1, 0);

            return panel;
        }
    }
}
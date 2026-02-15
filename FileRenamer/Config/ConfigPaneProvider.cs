using Jnot.FileRenamer.Config;
using Jnot.Shared.Config.IO;
using Jnot.Shared.Config.Migration;
using Jnot.Shared.Config.Models;
using Jnot.Shared.Config.Services;
using Jnot.Shared.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

public class ConfigPaneProvider : ITaskPaneContentProvider
{
    private readonly IFileRenamerConfigAdapter _adapter;
    private FileRenamerConfig _cfg;

    public string Title => "File Renamer Configuration";
    public string Status => "Edit settings and save";

    public ConfigPaneProvider(IFileRenamerConfigAdapter adapter)
    {
        _adapter = adapter;
        _cfg = adapter.GetConfig();
    }

    public Task PopulateAsync(Panel panel)
    {
        // No async needed — UI must be built on UI thread anyway
        if (panel.InvokeRequired)
        {
            System.Diagnostics.Debug.WriteLine("PopulateAsync: InvokeRequired");
            panel.BeginInvoke(new Action(() => BuildUI(panel)));
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("PopulateAsync: NOT InvokeRequired");
            BuildUI(panel);
        }

        return Task.CompletedTask;
    }

    private void BuildUI(Panel panel)
    {
        panel.Controls.Clear();

        var lblInput = new Label { Text = "Input Folder:", AutoSize = true, Top = 5, Left = 10 };
        var txtInput = new TextBox { Text = _cfg.InputFolder, Width = 260, Top = 30, Left = 10 };
        var btnInput = new Button { Text = "...", Width = 30, Top = 30, Left = 280 };

        btnInput.Click += (s, e) =>
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtInput.Text = dlg.SelectedPath;
        };

        var lblOutput = new Label { Text = "Output Folder:", AutoSize = true, Top = 65, Left = 10 };
        var txtOutput = new TextBox { Text = _cfg.OutputFolder, Width = 260, Top = 90, Left = 10 };
        var btnOutput = new Button { Text = "...", Width = 30, Top = 90, Left = 280 };

        btnOutput.Click += (s, e) =>
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtOutput.Text = dlg.SelectedPath;
        };

        var chkDebug = new CheckBox
        {
            Text = "Enable Debug Mode",
            Checked = _cfg.Debug,
            Top = 130,
            Left = 10,
            AutoSize = true
        };
        var chkDryRun = new CheckBox
        {
            Text = "Dry Run (simulate only)",
            Checked = _cfg.DryRun,
            Top = 160,
            Left = 10,
            AutoSize = true
        };
        var chkClearOnRun = new CheckBox
        {
            Text = "Clear On Run",
            Checked = _cfg.ClearOnRun,
            Top = 190,
            Left = 10,
            AutoSize = true
        };

        var btnSave = new Button
        {
            Text = "Save Configuration",
            Width = 150,
            Top = 230,
            Left = 10
        };

        btnSave.Click += (s, e) =>
        {
            try
            {
                _cfg.InputFolder = txtInput.Text;
                _cfg.OutputFolder = txtOutput.Text;
                _cfg.Debug = chkDebug.Checked;
                _cfg.DryRun = chkDryRun.Checked;
                _cfg.ClearOnRun = chkClearOnRun.Checked;

                var svc = new ConfigService(
                    new ConfigLoader(new ConfigMigrationEngine()),
                    new ConfigWriter()
                );

                svc.Save(new RootConfig
                {
                    FileRenamer = _cfg,
                    Title = "JNOT Global Configuration",
                    Version = "1.0"
                });

                MessageBox.Show("Configuration saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving configuration:\n" + ex.Message);
            }
        };

        panel.Controls.Add(lblInput);
        panel.Controls.Add(txtInput);
        panel.Controls.Add(btnInput);

        panel.Controls.Add(lblOutput);
        panel.Controls.Add(txtOutput);
        panel.Controls.Add(btnOutput);

        panel.Controls.Add(chkDebug);
        panel.Controls.Add(chkDryRun);
        panel.Controls.Add(chkClearOnRun);
        panel.Controls.Add(btnSave);
    }
}

using JNOT.FileRenamer.Config;
using JNOT.FileRenamer.Logging;
using JNOT.FileRenamer.UI;
using JNOT.Shared.Config;
using JNOT.Shared.Config.IO;
using JNOT.Shared.Config.Migration;
using JNOT.Shared.Config.Services;
using Microsoft.Office.Tools;
using System;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;

namespace AddIn
{
    public partial class ThisAddIn
    {
        private CustomTaskPane _fileRenamerPane;
        private CustomTaskPane _configPane;

        private IFileRenamerConfigAdapter _adapter;
        private Logger _logger;

        private void ThisAddIn_Startup(object sender, EventArgs e)
        {
            // Build config pipeline
            _adapter = new FileRenamerConfigAdapter(
                new ConfigService(
                    new ConfigLoader(new ConfigMigrationEngine()),
                    new ConfigWriter()
                )
            );

            // Logger requires an output folder
            var outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _logger = new Logger(outputFolder);
        }

        private void ThisAddIn_Shutdown(object sender, EventArgs e)
        {
        }

        protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new AddInRibbon();
        }

        public void ShowFileRenamerPane()
        {
            if (_fileRenamerPane == null)
            {
                var provider = new FileRenamerPaneProvider(_adapter, _logger);

                // Create a UserControl to host the panel
                var host = new UserControl();
                var panel = new Panel { Dock = DockStyle.Fill };
                host.Controls.Add(panel);

                _fileRenamerPane = this.CustomTaskPanes.Add(host, "File Renamer");
                _fileRenamerPane.Width = 400;

                provider.PopulateAsync(panel);
            }

            _fileRenamerPane.Visible = true;
        }

        public void ShowConfigPane()
        {
            if (_configPane == null)
            {
                var provider = new ConfigPaneProvider(_adapter);

                var host = new UserControl();
                var panel = new Panel { Dock = DockStyle.Fill };
                host.Controls.Add(panel);

                _configPane = this.CustomTaskPanes.Add(host, "Configuration");
                _configPane.Width = 400;

                provider.PopulateAsync(panel);
            }

            _configPane.Visible = true;
        }

        #region VSTO generated code
        private void InternalStartup()
        {
            this.Startup += ThisAddIn_Startup;
            this.Shutdown += ThisAddIn_Shutdown;
        }
        #endregion
    }
}
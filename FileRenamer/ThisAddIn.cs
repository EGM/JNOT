using JNOT.FileRenamer.Config;
using JNOT.FileRenamer.Logging;
using JNOT.FileRenamer.UI;
using JNOT.Shared.Config.IO;
using JNOT.Shared.Config.Migration;
using JNOT.Shared.Config.Services;
using JNOT.Shared.UI;
using Microsoft.Office.Tools.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;


namespace JNOT.FileRenamer
{
    
    public partial class ThisAddIn
    {
        private Microsoft.Office.Tools.CustomTaskPane _ctp;
        private Microsoft.Office.Tools.CustomTaskPane _configPane;
        private IFileRenamerConfigAdapter _adapter;
        private Logger _logger;
        private JnotTaskPane _pane;

        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new RibbonMain();
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            var configService = new ConfigService(
                new ConfigLoader(new ConfigMigrationEngine()),
                new ConfigWriter()
            );

            _adapter = new FileRenamerConfigAdapter(configService);
            _logger = new Logger(_adapter.GetConfig().OutputFolder);
            var provider = new FileRenamerPaneProvider(_adapter, _logger);

            var paneControl = new JnotTaskPane();

            // Load the provider into the pane
            // (fire and forget is fine here because PopulateAsync is UI-safe)
            _ = paneControl.LoadFromAsync(provider);

            // Add to VSTO task panes
            Globals.ThisAddIn.CustomTaskPanes.Add(
                paneControl,
                "File Renamer"
            );
        }

        public async void ShowFileRenamerPane()
        {
            var provider = new FileRenamerPaneProvider(_adapter, _logger);

            if (_ctp == null)
            {
                _pane = new JnotTaskPane();
                await _pane.LoadFromAsync(provider);

                _ctp = this.CustomTaskPanes.Add(_pane, "File Renamer");
                _ctp.Width = 400;
            }
            else
            {
                // Refresh the pane with updated config
                await _pane.LoadFromAsync(provider);
            }

            _ctp.Visible = true;
        }

        public void ShowConfigPane()
        {
            if (_configPane == null)
            {
                var pane = new JnotTaskPane();
                var provider = new ConfigPaneProvider(
                    new FileRenamerConfigAdapter(
                        new ConfigService(
                            new ConfigLoader(new ConfigMigrationEngine()),
                            new ConfigWriter()
                        )
                    )
                );

                _configPane = this.CustomTaskPanes.Add(pane, "Configuration");
                _configPane.Width = 420;

                // Load content asynchronously
                _ = pane.LoadFromAsync(provider);
            }

            _configPane.Visible = true;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}

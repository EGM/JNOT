using JNOT.FileRenamer.UI;
using Microsoft.Office.Tools.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using JNOT.Shared.UI;
using System.Windows.Forms;
using System.Threading.Tasks;


namespace JNOT.FileRenamer
{
    
    public partial class ThisAddIn
    {
        private Microsoft.Office.Tools.CustomTaskPane _ctp;
        private JnotTaskPane _pane;

        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new RibbonMain();
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
        }

        public void ShowFileRenamerPane()
        {
            if (_ctp == null)
            {
                var provider = new RenameFilesPaneProvider();

                _pane = new JnotTaskPane();
                _pane.LoadFromAsync(provider);

                _ctp = this.CustomTaskPanes.Add(_pane, provider.Title);
                _ctp.Width = 400;
            }

            _ctp.Visible = true;
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Office = Microsoft.Office.Core;

namespace AddIn
{
    [ComVisible(true)]
    public class AddInRibbon : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI _ribbon;

        public AddInRibbon()
        {
        }

        public string GetCustomUI(string ribbonID)
        {
            // Embedded resource name MUST match the actual namespace + folder + filename
            return LoadRibbonXml("AddIn.AddInRibbon.xml");
        }

        private string LoadRibbonXml(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Ribbon XML resource not found: " + resourceName);

                // Dump available resources to Output window
                foreach (var name in asm.GetManifestResourceNames())
                    System.Diagnostics.Debug.WriteLine("RESOURCE: " + name);

                return "<customUI xmlns='http://schemas.microsoft.com/office/2009/07/customui'></customUI>";
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            _ribbon = ribbonUI;
        }

        public void OpenFileRenamer_Click(Office.IRibbonControl control)
        {
            System.Diagnostics.Debug.WriteLine("OpenFileRenamer_Click fired");
            Globals.ThisAddIn.ShowFileRenamerPane();
        }

        public void OpenConfig_Click(Office.IRibbonControl control)
        {
            System.Diagnostics.Debug.WriteLine("OpenConfig_Click fired");
            Globals.ThisAddIn.ShowConfigPane();
        }

        public void OpenInfo_Click(Office.IRibbonControl control)
        {
            System.Diagnostics.Debug.WriteLine("OpenInfo_Click fired");
            Globals.ThisAddIn.ShowInfoPane();
            System.Diagnostics.Debug.WriteLine("After ThisAddIn.ShowInfoPane completes");
        }

        public void OpenHelp_Click(Office.IRibbonControl control)
        {
            System.Diagnostics.Debug.WriteLine("OpenHelp_Click fired");
            System.Diagnostics.Process.Start("https://egm.github.io/JNOT/");
        }

        //Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226

        #region Helpers

        public Bitmap GetImage(Office.IRibbonControl control)
        {
            return control.Id switch
            {
                "btnHelp" => LoadPng("AddIn.resources.help.png"),
                "btnInfo" => LoadPng("AddIn.resources.info.png"),
                "btnConfig" => LoadPng("AddIn.resources.gear.png"),
                "btnFileRenamer" => LoadPng("AddIn.resources.file.png"),
                _ => null
            };
        }
        
        private Bitmap LoadPng(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(resourceName);
            return stream != null ? new Bitmap(stream) : null;
        }

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}

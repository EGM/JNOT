using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Office = Microsoft.Office.Core;

namespace JNOT.FileRenamer
{
    [ComVisible(true)]
    public class RibbonMain : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI _ribbon;

        public RibbonMain()
        {
        }

        public string GetCustomUI(string ribbonID)
        {
            // Embedded resource name MUST match the actual namespace + folder + filename
            return LoadRibbonXml("JNOT.FileRenamer.Ribbon.xml");
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
    }
}
using System.IO;
using Office = Microsoft.Office.Core;

namespace JNOT.FileRenamer
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class RibbonMain : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI _ribbon;

        public string GetCustomUI(string ribbonID)
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream("JNOT.FileRenamer.Ribbon.xml");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            _ribbon = ribbonUI;
        }

        public void OpenFileRenamer_Click(Office.IRibbonControl control)
        {
            Globals.ThisAddIn.ShowFileRenamerPane();
        }
    }
}
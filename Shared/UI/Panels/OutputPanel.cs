using System.Windows.Forms;
using JNOT.Shared.UI.Controls;

namespace JNOT.Shared.UI.Panels
{
    public class OutputPanel : UserControl
    {
        private readonly JnotRichOutputBox _output = new JnotRichOutputBox();

        public bool ClearOnRun { get; set; } = true;

        public JnotRichOutputBox Output => _output;

        public OutputPanel()
        {
            _output.Dock = DockStyle.Fill;
            Controls.Add(_output);
        }

        public void PrepareForRun()
        {
            if (ClearOnRun)
                _output.Clear();
        }
    }
}
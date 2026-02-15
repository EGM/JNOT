using System.Windows.Forms;
using Jnot.Shared.UI.Controls;

namespace Jnot.Shared.UI.Controls.Panels
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

using System.Threading.Tasks;
using System.Windows.Forms;
using JNOT.Shared.UI;

namespace JNOT.FileRenamer.UI
{
    public class RenameFilesPaneProvider : ITaskPaneContentProvider
    {
        public string Title => "Rename Files";
        public string Status => "Ready";

        public async Task PopulateAsync(Panel contentPanel)
        {
            var label = new Label
            {
                Text = "This is RenameFiles content",
                AutoSize = true
            };

            contentPanel.Controls.Add(label);

            // Nothing async to do yet, but we satisfy the signature
            await Task.CompletedTask;
        }
    }
}
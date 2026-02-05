using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JNOT.Shared.UI
{
    public interface ITaskPaneContentProvider
    {
        string Title { get; }
        string Status { get; }

        /// <summary>
        /// Populate the pane asynchronously.
        /// </summary>
        Task PopulateAsync(Panel contentPanel);
    }
}
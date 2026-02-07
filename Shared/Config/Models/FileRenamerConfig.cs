
namespace JNOT.Shared.Config.Models
{
    public class FileRenamerConfig
    {
        public string InputFolder { get; set; } = "";
        public string OutputFolder { get; set; } = "";
        public bool Debug { get; set; } = false;
        public bool DryRun { get; set; } = false;
        public bool ClearOnRun { get; set; } = true;
    }
}

namespace JNOT.Shared.Config.Models
{
    public class RootConfig
    {
        public string Title { get; set; } = "JNOT Global Configuration";
        public string Version { get; set; } = "1.0";

        public FileRenamerConfig FileRenamer { get; set; } = new();
        // Future tools go here
    }
}


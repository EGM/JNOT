using JNOT.Shared.Config.Models;

namespace JNOT.FileRenamer.Config;

public interface IFileRenamerConfigAdapter
{
    FileRenamerConfig GetConfig();
}
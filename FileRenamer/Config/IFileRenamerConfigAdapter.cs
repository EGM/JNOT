using Jnot.Shared.Config.Models;

namespace Jnot.FileRenamer.Config;

public interface IFileRenamerConfigAdapter
{
    FileRenamerConfig GetConfig();
}

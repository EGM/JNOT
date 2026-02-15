using Jnot.Shared.Config.Models;
using Jnot.Shared.Config.Services;

namespace Jnot.FileRenamer.Config;

public class FileRenamerConfigAdapter : IFileRenamerConfigAdapter
{
    private readonly IConfigService _configService;

    public FileRenamerConfigAdapter(IConfigService configService)
    {
        _configService = configService;
    }

    public FileRenamerConfig GetConfig()
    {
        var root = _configService.LoadOrCreate();
        return root.FileRenamer;
    }
}

using JNOT.Shared.Config.Models;
using JNOT.Shared.Config.Services;

namespace JNOT.FileRenamer.Config;

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
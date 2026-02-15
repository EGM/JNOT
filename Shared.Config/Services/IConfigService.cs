using Jnot.Shared.Config.Models;

namespace Jnot.Shared.Config.Services;

public interface IConfigService
{
    RootConfig Load();
    RootConfig LoadOrCreate();
    void Save(RootConfig config);
}

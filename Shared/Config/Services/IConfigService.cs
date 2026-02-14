using JNOT.Shared.Config.Models;

namespace JNOT.Shared.Config.Services;

public interface IConfigService
{
    RootConfig Load();
    RootConfig LoadOrCreate();
    void Save(RootConfig config);
}

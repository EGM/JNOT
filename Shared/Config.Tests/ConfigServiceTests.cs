using Xunit;
using JNOT.Shared.Config.Services;
using JNOT.Shared.Config.IO;
using JNOT.Shared.Config.Migration;
using JNOT.Shared.Config.Tests.TestHelpers;
using System.IO;

namespace JNOT.Shared.Config.Tests;

public class ConfigServiceTests
{
    [Fact]
    public void LoadOrCreate_ReturnsConfig()
    {
        using var temp = new TempFile();

        var loader = new ConfigLoader(new ConfigMigrationEngine());
        var writer = new ConfigWriter();

        var service = new ConfigService(loader, writer);

        var cfg = service.LoadOrCreate();

        Assert.NotNull(cfg);
        Assert.NotNull(cfg.FileRenamer);
    }

    [Fact]
    public void Save_ThrowsOnInvalidConfig()
    {
        using var temp = new TempFile();

        var loader = new ConfigLoader(new ConfigMigrationEngine());
        var writer = new ConfigWriter();

        var service = new ConfigService(loader, writer);

        var cfg = service.LoadOrCreate();
        cfg.FileRenamer.InputFolder = "";

        Assert.Throws<InvalidOperationException>(() => service.Save(cfg));
    }
}
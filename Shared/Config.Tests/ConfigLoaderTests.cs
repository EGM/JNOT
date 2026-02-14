using Xunit;
using JNOT.Shared.Config.IO;
using JNOT.Shared.Config.Migration;
using JNOT.Shared.Config.Models;
using JNOT.Shared.Config.Defaults;
using JNOT.Shared.Config.Tests.TestHelpers;
using System.IO;

namespace JNOT.Shared.Config.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void LoadOrCreate_CreatesDefaultConfig_WhenFileMissing()
    {
        var migration = new ConfigMigrationEngine();
        var loader = new ConfigLoader(migration);

        using var temp = new TempFile();
        File.Delete(temp.Path);

        var cfg = loader.LoadOrCreate(temp.Path);

        Assert.Equal(ConfigDefaults.CurrentVersion, cfg.Version);
        Assert.NotNull(cfg.FileRenamer);
    }

    [Fact]
    public void LoadOrCreate_LoadsExistingConfig()
    {
        using var temp = new TempFile();

        File.WriteAllText(temp.Path, @"
title = ""Test""
version = ""1.0""

[FileRenamer]
input_folder = ""A""
output_folder = ""B""
debug = true
");

        var migration = new ConfigMigrationEngine();
        var loader = new ConfigLoader(migration);

        var cfg = loader.LoadOrCreate(temp.Path);

        Assert.Equal("Test", cfg.Title);
        Assert.Equal("A", cfg.FileRenamer.InputFolder);
        Assert.True(cfg.FileRenamer.Debug);
    }

    [Fact]
    public void LoadOrCreate_RunsMigrationSteps()
    {
        using var temp = new TempFile();

        File.WriteAllText(temp.Path, @"
title = ""Test""
version = ""1.0""

[FileRenamer]
input_folder = ""A""
output_folder = ""B""
");

        var step = new TestConfigMigrationStep();

        // FIX: explicitly type the array as IConfigMigrationStep[]
        var migration = new ConfigMigrationEngine(
            new IConfigMigrationStep[] { step }
        );

        var loader = new ConfigLoader(migration);

        var cfg = loader.LoadOrCreate(temp.Path);
        Assert.Equal("1.1", cfg.Version);

        Console.WriteLine($"Version loaded: '{cfg.Version}'");
        System.Diagnostics.Debug.WriteLine($"Loaded version: '{cfg.Version}'");

        Assert.True(step.Applied);
        Assert.Equal("1.1", cfg.Version);
        Console.WriteLine($"Version loaded: '{cfg.Version}'");
        System.Diagnostics.Debug.WriteLine($"Loaded version: '{cfg.Version}'");
    }
}

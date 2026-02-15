
## 📁 Directory: /

- ConfigLoaderTests.cs

```cs
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

```

- ConfigServiceTests.cs

```cs
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

```

- ConfigWriterTests.cs

```cs
using Xunit;
using JNOT.Shared.Config.IO;
using JNOT.Shared.Config.Models;
using System.IO;
using JNOT.Shared.Config.Tests.TestHelpers;

namespace JNOT.Shared.Config.Tests;

public class ConfigWriterTests
{
    [Fact]
    public void Save_WritesTOMLFile()
    {
        using var temp = new TempFile();

        var writer = new ConfigWriter();

        var cfg = new RootConfig
        {
            Title = "Test",
            Version = "1.0",
            FileRenamer = new FileRenamerConfig
            {
                InputFolder = "A",
                OutputFolder = "B",
                Debug = true
            }
        };

        writer.Save(temp.Path, cfg);

        var text = File.ReadAllText(temp.Path);

        Assert.Contains("title = \"Test\"", text);
        Assert.Contains("[FileRenamer]", text);
        Assert.Contains("input_folder = \"A\"", text);
    }

    [Fact]
    public void Save_PreservesUnknownSections()
    {
        using var temp = new TempFile();

        File.WriteAllText(temp.Path, @"
[UnknownSection]
foo = ""bar""
");

        var writer = new ConfigWriter();

        var cfg = new RootConfig();

        writer.Save(temp.Path, cfg);

        var text = File.ReadAllText(temp.Path);

        Assert.Contains("[UnknownSection]", text);
        Assert.Contains("foo = \"bar\"", text);
    }
}

```

- Shared.Config.Tests.csproj

## 📁 Directory: TestHelpers

- TestHelpers\TempFile.cs

```cs
using System;
using System.IO;

namespace JNOT.Shared.Config.Tests.TestHelpers;

public sealed class TempFile : IDisposable
{
    public string Path { get; }

    public TempFile()
    {
        Path = System.IO.Path.GetTempFileName();
    }

    public void Dispose()
    {
        if (File.Exists(Path))
            File.Delete(Path);
    }
}

```

- TestHelpers\TestConfigMigrationStep.cs

```cs
using JNOT.Shared.Config.Models;
using JNOT.Shared.Config.Migration;

namespace JNOT.Shared.Config.Tests.TestHelpers;

public class TestConfigMigrationStep : IConfigMigrationStep
{
    public string FromVersion => "1.0";
    public string ToVersion => "1.1";

    public bool Applied { get; private set; }

    public void Apply(RootConfig config)
    {
        Applied = true;
        config.Version = "1.1";
    }
}

```

- xunit.runner.json
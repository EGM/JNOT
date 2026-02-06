
## 📁 Directory: /


## 📁 Directory: Defaults

- Defaults\ConfigDefaults.cs

```cs
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JNOT.Shared.Config.Models;

namespace JNOT.Shared.Config.Defaults
{
    public static class ConfigDefaults
    {
        public const string CurrentVersion = "1.0";

        public static void ApplyDefaults(RootConfig config)
        {
            config.Title ??= "JNOT Global Configuration";
            config.Version ??= CurrentVersion;

            config.FileRenamer ??= new FileRenamerConfig();
            ApplyFileRenamerDefaults(config.FileRenamer);
        }

        private static void ApplyFileRenamerDefaults(FileRenamerConfig cfg)
        {
            cfg.InputFolder ??= string.Empty;
            cfg.OutputFolder ??= string.Empty;
            // Debug default is already false
        }
    }
}

```


## 📁 Directory: IO

- IO\ConfigFileLocator.cs

```cs
﻿using System;
using System.IO;

namespace JNOT.Shared.Config.IO
{
    public static class ConfigFileLocator
    {
        private const string FileName = "jnot.config.toml";

        public static string GetConfigPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "JNOT");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, FileName);
        }
    }
}
```

- IO\ConfigLoader.cs

```cs
﻿using System.IO;
using JNOT.Shared.Config.Models;
using JNOT.Shared.Config.Defaults;
using JNOT.Shared.Config.Migration;
using Tomlyn;
using Tomlyn.Model;

namespace JNOT.Shared.Config.IO
{
    public class ConfigLoader
    {
        private readonly ConfigMigrationEngine _migrationEngine;

        public ConfigLoader(ConfigMigrationEngine migrationEngine)
        {
            _migrationEngine = migrationEngine;
        }

        public RootConfig LoadOrCreate(string path)
        {
            if (!File.Exists(path))
            {
                var cfg = new RootConfig();
                ConfigDefaults.ApplyDefaults(cfg);
                _migrationEngine.MigrateIfNeeded(cfg);
                return cfg;
            }

            var text = File.ReadAllText(path);
            var model = Toml.ToModel<TomlTable>(text);

            var config = new RootConfig
            {
                Title = ReadString(model, "title", "JNOT Global Configuration"),
                Version = ReadString(model, "version", "1.0"),
                FileRenamer = LoadFileRenamer(model)
            };

            ConfigDefaults.ApplyDefaults(config);
            _migrationEngine.MigrateIfNeeded(config);

            return config;
        }

        private static FileRenamerConfig LoadFileRenamer(TomlTable root)
        {
            if (!root.TryGetValue("FileRenamer", out var sectionObj) ||
                sectionObj is not TomlTable section)
            {
                return new FileRenamerConfig();
            }

            return new FileRenamerConfig
            {
                InputFolder = ReadString(section, "input_folder", ""),
                OutputFolder = ReadString(section, "output_folder", ""),
                Debug = ReadBool(section, "debug", false)
            };
        }

        // -----------------------------
        // TOML VALUE HELPERS
        // -----------------------------

        private static string ReadString(TomlTable table, string key, string fallback)
        {
            if (!table.TryGetValue(key, out var value) || value is null)
                return fallback;

            return value as string ?? value.ToString() ?? fallback;
        }

        private static bool ReadBool(TomlTable table, string key, bool fallback)
        {
            if (!table.TryGetValue(key, out var value) || value is null)
                return fallback;

            if (value is bool b)
                return b;

            return fallback;
        }

    }
}
```

- IO\ConfigWriter.cs

```cs
﻿using System.IO;
using JNOT.Shared.Config.Models;
using Tomlyn;
using Tomlyn.Model;

namespace JNOT.Shared.Config.IO;

public class ConfigWriter
{
    public void Save(string path, RootConfig config)
    {
        TomlTable root;

        if (File.Exists(path))
        {
            var existing = File.ReadAllText(path);
            root = Toml.ToModel<TomlTable>(existing);
        }
        else
        {
            root = new TomlTable();
        }

        // Update scalar fields
        root["title"] = config.Title;
        root["version"] = config.Version;

        // FileRenamer section
        if (!root.TryGetValue("FileRenamer", out var frObj) || frObj is not TomlTable frSection)
        {
            frSection = new TomlTable();
            root["FileRenamer"] = frSection;
        }

        frSection["input_folder"] = config.FileRenamer.InputFolder;
        frSection["output_folder"] = config.FileRenamer.OutputFolder;
        frSection["debug"] = config.FileRenamer.Debug;

        var text = Toml.FromModel(root);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }
}
```


## 📁 Directory: Migration

- Migration\ConfigMigrationEngine.cs

```cs
﻿using JNOT.Shared.Config.Defaults;
using JNOT.Shared.Config.Migration;
using JNOT.Shared.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JNOT.Shared.Config.Migration;

public class ConfigMigrationEngine
{
    private readonly List<IConfigMigrationStep> _steps = new();

    public ConfigMigrationEngine(IEnumerable<IConfigMigrationStep>? steps = null)
    {
        if (steps != null)
            _steps.AddRange(steps);
    }

    public void MigrateIfNeeded(RootConfig config)
    {
        var version = (config.Version ?? "1.0").Trim();

        foreach (var step in _steps)
        {
            if (version == step.FromVersion.Trim())
            {
                step.Apply(config);
                version = step.ToVersion.Trim();
            }
        }

        config.Version = version;
    }
}
```

- Migration\IConfigMigrationStep.cs

```cs
﻿using JNOT.Shared.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JNOT.Shared.Config.Migration
{
    public interface IConfigMigrationStep
    {
        string FromVersion { get; }
        string ToVersion { get; }

        void Apply(RootConfig config);
    }
}
```


## 📁 Directory: Models

- Models\FileRenamerConfig.cs

```cs
﻿
namespace JNOT.Shared.Config.Models
{
    public class FileRenamerConfig
    {
        public string InputFolder { get; set; } = "";
        public string OutputFolder { get; set; } = "";
        public bool Debug { get; set; } = false;
    }
}
```

- Models\RootConfig.cs

```cs
﻿
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

```


## 📁 Directory: Properties

- Properties\AssemblyInfo.cs

```cs
﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Shared.Config")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Shared.Config")]
[assembly: AssemblyCopyright("Copyright ©  2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6c05ad0c-d60f-4181-a021-0609884ba658")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

```


## 📁 Directory: Services

- Services\ConfigService.cs

```cs
﻿using JNOT.Shared.Config.IO;
using JNOT.Shared.Config.Models;
using JNOT.Shared.Config.Validation;
using System;

namespace JNOT.Shared.Config.Services;

public class ConfigService : IConfigService
{
    private readonly ConfigLoader _loader;
    private readonly ConfigWriter _writer;
    private readonly string _path;

    public ConfigService(ConfigLoader loader, ConfigWriter writer)
    {
        _loader = loader;
        _writer = writer;
        _path = ConfigFileLocator.GetConfigPath();
    }

    public RootConfig Load()
    {
        return _loader.LoadOrCreate(_path);
    }

    public RootConfig LoadOrCreate()
    {
        return _loader.LoadOrCreate(_path);
    }

    public void Save(RootConfig config)
    {
        var validation = ConfigValidator.Validate(config);
        if (!validation.IsValid)
        {
            var message = string.Join(Environment.NewLine, validation.Errors);
            throw new InvalidOperationException($"Config is invalid:{Environment.NewLine}{message}");
        }

        _writer.Save(_path, config);
    }
}
```

- Services\IConfigService.cs

```cs
﻿using JNOT.Shared.Config.Models;

namespace JNOT.Shared.Config.Services;

public interface IConfigService
{
    RootConfig Load();
    RootConfig LoadOrCreate();
    void Save(RootConfig config);
}
```

- Shared.Config.csproj

## 📁 Directory: Validation

- Validation\ConfigValidationResult.cs

```cs
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JNOT.Shared.Config.Validation
{    
    public class ConfigValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new();
    }
}

```

- Validation\ConfigValidator.cs

```cs
﻿using JNOT.Shared.Config.Models;

namespace JNOT.Shared.Config.Validation
{
    public static class ConfigValidator
    {
        public static ConfigValidationResult Validate(RootConfig config)
        {
            var result = new ConfigValidationResult();

            if (string.IsNullOrWhiteSpace(config.Title))
                result.Errors.Add("Title must not be empty.");

            if (config.FileRenamer is null)
            {
                result.Errors.Add("FileRenamer section is missing.");
            }
            else
            {
                ValidateFileRenamer(config.FileRenamer, result);
            }

            return result;
        }

        private static void ValidateFileRenamer(FileRenamerConfig cfg, ConfigValidationResult result)
        {
            // These can be relaxed later if you want optional folders
            if (string.IsNullOrWhiteSpace(cfg.InputFolder))
                result.Errors.Add("[FileRenamer] InputFolder must not be empty.");

            if (string.IsNullOrWhiteSpace(cfg.OutputFolder))
                result.Errors.Add("[FileRenamer] OutputFolder must not be empty.");
        }
    }
}
```

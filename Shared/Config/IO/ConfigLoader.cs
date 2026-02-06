using System.IO;
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
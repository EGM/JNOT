using System.IO;
using Jnot.Shared.Config.Models;
using Tomlyn;
using Tomlyn.Model;

namespace Jnot.Shared.Config.IO;

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
        frSection["dry_run"] = config.FileRenamer.DryRun;
        frSection["clear_on_run"] = config.FileRenamer.ClearOnRun;

        var text = Toml.FromModel(root);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }
}

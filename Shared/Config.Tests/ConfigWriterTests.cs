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

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

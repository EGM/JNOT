using Jnot.Shared.Config.Defaults;
using Jnot.Shared.Config.Migration;
using Jnot.Shared.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jnot.Shared.Config.Migration;

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

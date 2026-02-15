using Jnot.Shared.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jnot.Shared.Config.Migration
{
    public interface IConfigMigrationStep
    {
        string FromVersion { get; }
        string ToVersion { get; }

        void Apply(RootConfig config);
    }
}

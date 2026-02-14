using System;
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


using System;
using System.IO;

namespace Jnot.Shared.Config.IO
{
    public static class ConfigFileLocator
    {
        private const string FileName = "Jnot.config.toml";

        public static string GetConfigPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "Jnot");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, FileName);
        }
    }
}

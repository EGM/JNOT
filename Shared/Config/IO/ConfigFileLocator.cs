using System;
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

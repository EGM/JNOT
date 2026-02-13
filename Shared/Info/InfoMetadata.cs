using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace JNOT.Shared.Info
{
    public static class InfoMetadata
    {
        public static string Version =>
            Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

        public static string BuildDate =>
            File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location)
                .ToString("yyyy-MM-dd HH:mm");

        public static string ConfigPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "JNOT", "config.toml");

        public static string LogFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                         "JNOT", "Logs");

        public static string[] Components => new[]
        {
            "File Renamer",
            "Shared.UI",
            "Shared.Config",
            "Shared.Logging",
            "Shared.Info"
        };

        // Diagnostics
        public static string DotNetVersion =>
            Environment.Version.ToString();

        public static string OSVersion =>
            Environment.OSVersion.ToString();

        public static string ProcessBitness =>
            Environment.Is64BitProcess ? "64-bit" : "32-bit";

        public static string MemoryUsage =>
            $"{Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} MB";
    }
}
using System;
using System.IO;

namespace JNOT.FileRenamer.Logging
{
    public class Logger
    {
        private readonly string _logPath;

        public Logger(string outputFolder)
        {
            Directory.CreateDirectory(Path.Combine(outputFolder, "logs"));
            _logPath = Path.Combine(outputFolder, "logs",
                $"{DateTime.Now:yyyyMMdd-HHmmss}-run.log");
        }

        public void Log(string message)
        {
            File.AppendAllText(_logPath,
                $"{DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}");
        }
    }
}
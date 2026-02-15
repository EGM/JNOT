using System.IO;

namespace Jnot.Shared.FileSystem
{
    public class OutputFolderWriter
    {
        public string BuildOutputPath(string outputFolder, string finalName)
        {
            Directory.CreateDirectory(outputFolder);
            return Path.Combine(outputFolder, finalName);
        }
    }
}

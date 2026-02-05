using System.IO;

namespace JNOT.FileRenamer.FileSystem
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
using System;
using System.IO;

namespace JNOT.FileRenamer.FileSystem
{
    public class SafeRenameService
    {
        public void Rename(string sourcePath, string destPath)
        {
            if (!File.Exists(sourcePath))
                return;

            if (File.Exists(destPath))
                File.Delete(destPath);

            File.Move(sourcePath, destPath);
        }

        public void RenamePdfIfExists(string sourceXlsx, string destXlsx)
        {
            string srcPdf = Path.ChangeExtension(sourceXlsx, ".pdf");
            string dstPdf = Path.ChangeExtension(destXlsx, ".pdf");

            if (File.Exists(srcPdf))
                Rename(srcPdf, dstPdf);
        }
    }
}
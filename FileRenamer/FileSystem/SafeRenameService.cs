using System;
using System.IO;

namespace JNOT.FileRenamer.FileSystem
{
    public class SafeRenameService
    {
        public void Rename(string sourcePath, string destPath, bool dryRun)
        {
            if (!File.Exists(sourcePath))
                return;

            // DRY RUN — simulate only
            if (dryRun)
            {
                // No file operations — caller logs the message
                return;
            }

            if (File.Exists(destPath))
                File.Delete(destPath);

            File.Move(sourcePath, destPath);
        }

        public void RenamePdfIfExists(string sourceXlsx, string destXlsx, bool dryRun)
        {
            string srcPdf = Path.ChangeExtension(sourceXlsx, ".pdf");
            string dstPdf = Path.ChangeExtension(destXlsx, ".pdf");

            if (File.Exists(srcPdf))
                Rename(srcPdf, dstPdf, dryRun);
        }
    }
}
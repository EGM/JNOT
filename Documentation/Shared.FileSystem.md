
## 📁 Directory: /

- InputFolderScanner.cs

```cs
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jnot.Shared.FileSystem
{
    public class InputFolderScanner
    {
        public List<string> Scan(string inputFolder)
        {
            var list = new List<string>();

            if (!Directory.Exists(inputFolder))
                return list;

            return Directory
                .GetFiles(inputFolder, "*.xlsx")
                .Where(f =>
                {
                    var name = Path.GetFileName(f);
                    return name.EndsWith("_FLPivot.xlsx", System.StringComparison.OrdinalIgnoreCase)
                        || name.IndexOf("_FLPivot", System.StringComparison.OrdinalIgnoreCase) >= 0;
                })
                .ToList();
        }
    }
}

```

- OutputFolderWriter.cs

```cs
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

```

- SafeRenameService.cs

```cs
using System;
using System.IO;

namespace Jnot.Shared.FileSystem
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

```

- Shared.FileSystem.csproj
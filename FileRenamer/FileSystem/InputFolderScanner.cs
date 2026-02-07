using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JNOT.FileRenamer.FileSystem
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
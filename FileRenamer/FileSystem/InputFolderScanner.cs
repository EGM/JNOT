using System.Collections.Generic;
using System.IO;

namespace JNOT.FileRenamer.FileSystem
{
    public class InputFolderScanner
    {
        public List<string> Scan(string inputFolder)
        {
            var list = new List<string>();

            if (!Directory.Exists(inputFolder))
                return list;

            foreach (var file in Directory.GetFiles(inputFolder, "*.xlsx"))
                list.Add(file);

            return list;
        }
    }
}
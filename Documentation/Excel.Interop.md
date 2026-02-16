
## 📁 Directory: /

- Excel.Interop.csproj
- ExcelReader.cs

```cs
using System;
using Microsoft.Office.Interop.Excel;

namespace Jnot.Excel.Interop
{
    public class ExcelReader
    {
        private readonly PivotParser _parser = new PivotParser();

        public PivotData ReadPivot(string filePath)
        {
            Application app = null;
            Workbook wb = null;

            try
            {
                app = new Application { Visible = false, DisplayAlerts = false };
                wb = app.Workbooks.Open(filePath);

                Worksheet ws = wb.Sheets[1];
                Range used = ws.UsedRange;

                int rows = used.Rows.Count;
                int cols = used.Columns.Count;

                // Build a string[,] grid for the parser
                var grid = new string[rows, cols];

                for (int r = 1; r <= rows; r++)
                {
                    for (int c = 1; c <= cols; c++)
                    {
                        object raw = used.Cells[r, c].Value2;
                        grid[r - 1, c - 1] = raw?.ToString() ?? "";
                    }
                }

                // Sample date is always at C4 (row 4, col 3 in Excel → [3,2] in zero-based)
                string sampleDate = grid[3, 2];

                // Job number comes from filename
                string jobNumber = ExtractJobNumberFromFilename(filePath);

                // Parse pivot using pure logic
                return _parser.Parse(grid, sampleDate, jobNumber);
            }
            finally
            {
                wb?.Close(false);
                app?.Quit();
            }
        }

        private string ExtractJobNumberFromFilename(string filePath)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(filePath);
            var parts = name.Split('_');
            return parts.Length > 0 ? parts[0] : string.Empty;
        }
    }
}

```

- ExcelWorkbookDataSource.cs

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jnot.Excel.Interop
{
    public class ExcelWorkbookDataSource : IWorkbookDataSource
    {
        // Excel logic here
    }
}

```

- HeaderBlock.cs

```cs

using System;
using System.Collections.Generic;

namespace Jnot.Excel.Interop
{
    public class HeaderBlock
    {
        public string[] SiteIds { get; }
        public string SampleDate { get; }
        public string JobNumber { get; }

        private HeaderBlock(string[] siteIds, string sampleDate, string jobNumber)
        {
            SiteIds = siteIds;
            SampleDate = sampleDate;
            JobNumber = jobNumber;
        }

        public static HeaderBlock Parse(string[,] grid, int headerRow)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            string[] siteIds = Array.Empty<string>();
            string sampleDate = "";
            string jobNumber = "";

            // Walk upward until "Sample ID" is found
            for (int r = headerRow - 1; r >= 0; r--)
            {
                string label = grid[r, 0]?.Trim() ?? "";

                if (label.Equals("Sample ID", StringComparison.OrdinalIgnoreCase))
                {
                    var sites = new List<string>();
                    for (int c = 2; c < cols; c++)
                    {
                        string site = grid[r, c]?.Trim() ?? "";
                        if (!string.IsNullOrWhiteSpace(site))
                            sites.Add(site);
                    }
                    siteIds = sites.ToArray();
                }
                else if (label.Equals("Sample Collection Date", StringComparison.OrdinalIgnoreCase))
                {
                    sampleDate = grid[r, 2]?.Trim() ?? "";
                }
                else if (label.Equals("Laboratory Order Number", StringComparison.OrdinalIgnoreCase))
                {
                    jobNumber = grid[r, 2]?.Trim() ?? "";
                }

                // Stop if we reached the top or found everything
                if (r == 0)
                    break;
            }

            return new HeaderBlock(siteIds, sampleDate, jobNumber);
        }
    }
}

```

- IWorkbookDataSource.cs

```cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jnot.Excel.Interop
{
    public interface IWorkbookDataSource
    {
        //PivotData GetPivotData();
        //IEnumerable<ParameterBlock> GetParameters();
        //HeaderBlock GetHeader();
        //WorkbookMetadata GetMetadata();
    }
}

```

- ParameterBlock.cs

```cs
using System;
using System.Collections.Generic;

namespace Jnot.Excel.Interop
{
    public class ParameterBlock
    {
        public static List<PivotPair> Parse(
            string[,] grid,
            int headerRow,
            string[] siteIds)
        {
            var list = new List<PivotPair>();

            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            int row = headerRow + 1;

            while (row < rows)
            {
                string colA = grid[row, 0]?.Trim() ?? "";

                // Stop at Notes:
                if (colA.StartsWith("Notes:", StringComparison.OrdinalIgnoreCase))
                    break;

                // Skip blank rows
                if (string.IsNullOrWhiteSpace(colA))
                {
                    row++;
                    continue;
                }

                string colB = grid[row, 1]?.Trim() ?? "";

                // Skip method headers (A has value, B empty)
                if (!string.IsNullOrWhiteSpace(colA) &&
                    string.IsNullOrWhiteSpace(colB))
                {
                    row++;
                    continue;
                }

                // Parameter row (A and B have values)
                if (!string.IsNullOrWhiteSpace(colA) &&
                    !string.IsNullOrWhiteSpace(colB))
                {
                    string parameter = colA;

                    for (int i = 0; i < siteIds.Length; i++)
                    {
                        int col = 2 + i;
                        if (col >= cols)
                            continue;

                        string value = grid[row, col]?.Trim() ?? "";

                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            list.Add(new PivotPair(parameter, siteIds[i]));
                        }
                    }
                }

                row++;
            }

            return list;
        }
    }
}

```

- ParamSitePair.cs

```cs
namespace Jnot.Excel.Interop
{
    public class ParamSitePair
    {
        public string Parameter { get; }
        public string SiteId { get; }

        public ParamSitePair(string parameter, string siteId)
        {
            Parameter = parameter;
            SiteId = siteId;
        }
    }
}

```

- PivotData.cs

```cs
using System.Collections.Generic;

namespace Jnot.Excel.Interop
{
    public class PivotData
    {
        public string SampleDateRaw { get; set; } = "";
        public string JobNumberRaw { get; set; } = "";
        public List<PivotPair> Pairs { get; set; } = new();

        public PivotData() { }

        public PivotData(string sampleDateRaw, List<PivotPair> pairs)
        {
            SampleDateRaw = sampleDateRaw;
            Pairs = pairs;
        }

        public PivotData(string sampleDateRaw, string jobNumberRaw, List<PivotPair> pairs)

        {
            SampleDateRaw = sampleDateRaw;
            JobNumberRaw = jobNumberRaw;
            Pairs = pairs;
        }
    }
    public class PivotPair
    {
        public string Parameter { get; set; } = "";
        public string SiteId { get; set; } = "";

        public PivotPair() { }

        public PivotPair(string parameter, string siteID)
        {
            Parameter = parameter;
            SiteId = siteID;
        }
        
   }
}


```

- PivotParser.cs

```cs
using System;
using System.Collections.Generic;

namespace Jnot.Excel.Interop
{
    public class PivotParser
    {
        public PivotData Parse(string[,] grid, string sampleDateOverride, string jobNumberOverride)
        {
            int rows = grid.GetLength(0);

            // 1. Find header row ("Parameter")
            int headerRow = FindHeaderRow(grid, rows);
            if (headerRow == -1)
                return new PivotData(sampleDateOverride, jobNumberOverride, new List<PivotPair>());

            // 2. Parse header block (site IDs, sample date, job number)
            var header = HeaderBlock.Parse(grid, headerRow);

            // Allow overrides from ExcelReader
            string sampleDate = string.IsNullOrWhiteSpace(header.SampleDate)
                ? sampleDateOverride
                : header.SampleDate;

            string jobNumber = string.IsNullOrWhiteSpace(header.JobNumber)
                ? jobNumberOverride
                : header.JobNumber;

            // 3. Parse parameter block
            var pairs = ParameterBlock.Parse(grid, headerRow, header.SiteIds);

            return new PivotData(sampleDate, jobNumber, pairs);
        }

        private int FindHeaderRow(string[,] grid, int rows)
        {
            for (int r = 0; r < rows; r++)
            {
                string cell = grid[r, 0]?.Trim() ?? "";
                if (cell.Equals("Parameter", StringComparison.OrdinalIgnoreCase))
                    return r;
            }
            return -1;
        }
    }
}

```


## 📁 Directory: Properties

- Properties\AssemblyInfo.cs

```cs
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Jnot.Excel.Interop")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Jnot.Excel.Interop")]
[assembly: AssemblyCopyright("Copyright ©  2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("69c70291-26cf-43a9-ac5d-55c9db560ccb")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

```

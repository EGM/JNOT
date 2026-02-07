
## 📁 Directory: /


## 📁 Directory: Business

- Business\Pattern.cs

```cs
using System.Collections.Generic;

namespace JNOT.FileRenamer.Business
{
    public class Pattern
    {
        public string TypeCode { get; set; } = string.Empty;

        // These allow duplicates and preserve order
        public List<string> RequiredSites { get; } = new();
        public List<string> RequiredParameters { get; } = new();

        // This MUST allow duplicates — Weekly and Monthly depend on it
        public List<(string Parameter, string SiteId)> RequiredPairs { get; } = new();

        // Cardinality constraints
        public int? RequiredSiteCount { get; set; }
        public int? RequiredParameterCount { get; set; }
    }
}
```

- Business\PatternEngine.cs

```cs
using JNOT.FileRenamer.ExcelInterop;
using System.Linq;
using Tomlyn;

namespace JNOT.FileRenamer.Business
{
    public class PatternEngine
    {
        private readonly Pattern[] _patterns;

        public PatternEngine()
        {
            _patterns = new[]
            {
                BuildWeekendPattern(),
                BuildDailyPattern(),
                BuildWeeklyPattern(),
                BuildMonthlyPattern()
            };
        }

        public string ResolveTypeCode(PivotData data)
        {
            foreach (var p in _patterns)
            {
                if (Matches(p, data))
                    return p.TypeCode;
            }

            return "X";
        }

        private bool Matches(Pattern p, PivotData data)
        {
            var sites = data.Pairs.Select(x => x.SiteId).ToList();
            var distinctSites = sites.Distinct().ToList();

            var parameters = data.Pairs.Select(x => x.Parameter).ToList();
            var distinctParams = parameters.Distinct().ToList();

            // Cardinality checks
            if (p.RequiredSiteCount.HasValue &&
                distinctSites.Count != p.RequiredSiteCount.Value)
                return false;

            if (p.RequiredParameterCount.HasValue &&
                distinctParams.Count != p.RequiredParameterCount.Value)
                return false;

            // RequiredSites as a list (duplicates allowed)
            if (!p.RequiredSites.All(rs => distinctSites.Contains(rs)))
                return false;

            if (!p.RequiredParameters.All(rp => distinctParams.Contains(rp)))
                return false;

            // Required (Parameter, SiteId) pairs
            foreach (var kv in p.RequiredPairs)
            {
                if (!data.Pairs.Any(x => x.Parameter == kv.Parameter && x.SiteId == kv.SiteId))
                    return false;
            }
            return true;
        }

        private Pattern BuildWeekendPattern()
        {
            return new Pattern
            {
                TypeCode = "S",
                RequiredSites = { "EFA-2" },
                RequiredParameters = { "Total Suspended Solids" },
                RequiredPairs = { ( "Total Suspended Solids", "EFA-2" ) },
                RequiredSiteCount = 1,
                RequiredParameterCount = 1
            };
        }

        private Pattern BuildDailyPattern()
        {
            return new Pattern
            {
                TypeCode = "D",
                RequiredSites = { "EFA-2", "EFA-2", "EFA-2" }, // now preserved
                RequiredParameters =
                {
                    "Total Suspended Solids",
                    "Carbonaceous Biochemical Oxygen Demand",
                    "Coliform, Fecal"
                },
                RequiredPairs =
                {
                    ( "Total Suspended Solids", "EFA-2" ),
                    ( "Carbonaceous Biochemical Oxygen Demand", "EFA-2" ),
                    ( "Coliform, Fecal", "EFA-2" )
                },
                RequiredSiteCount = 1,
                RequiredParameterCount = 3
            };
        }

        private Pattern BuildWeeklyPattern()
        {
            return new Pattern
            {
                TypeCode = "W",
                RequiredSites = { "INF-1", "EFA-1", "EFA-1 CCC #1", "EFA-1 CCC #2" },
                RequiredParameters =
                {
                    "Total Suspended Solids",
                    "Carbonaceous Biochemical Oxygen Demand",
                    "Coliform, Fecal"
                },
                RequiredPairs =
                {
                    ( "Total Suspended Solids", "INF-1" ),
                    ( "Total Suspended Solids", "EFA-1" ),
                    ( "Carbonaceous Biochemical Oxygen Demand", "INF-1" ),
                    ( "Coliform, Fecal", "EFA-1 CCC #1" ),
                    ( "Coliform, Fecal", "EFA-1 CCC #2" )
                },
                RequiredSiteCount = 4,
                RequiredParameterCount = 3
            };
        }

        private Pattern BuildMonthlyPattern()
        {
            return new Pattern
            {
                TypeCode = "M",
                RequiredSites = { "INF-1", "EFA-1", "EFA-2" },
                RequiredParameters =
                {
                    "Total Dissolved Solids",
                    "Ammonia (as N)",
                    "Nitrogen, Kjeldahl",
                    "Orthophosphate as P, Dissolved",
                    "Total Phosphorus as P",
                    "Nitrogen, Organic",
                    "Nitrogen, Total",
                    "Nitrate as N",
                    "Nitrate Nitrite as N",
                    "Nitrite as N"
                },
                RequiredPairs =
                {
                    ( "Total Dissolved Solids", "EFA-1" ),
                    ( "Ammonia (as N)","INF-1" ),
                    ( "Ammonia (as N)","EFA-2" ),
                    ( "Nitrogen, Kjeldahl","INF-1" ),
                    ( "Nitrogen, Kjeldahl","EFA-2" ),
                    ( "Orthophosphate as P, Dissolved","INF-1" ),
                    ( "Orthophosphate as P, Dissolved","EFA-2" ),
                    ( "Total Phosphorus as P","INF-1" ),
                    ( "Total Phosphorus as P","EFA-2" ),
                    ( "Nitrogen, Organic","INF-1" ),
                    ( "Nitrogen, Organic","EFA-2" ),
                    ( "Nitrogen, Total","INF-1" ),
                    ( "Nitrogen, Total","EFA-2" ),
                    ( "Nitrate as N", "INF-1"),
                    ( "Nitrate as N","EFA-2" ),
                    ( "Nitrate Nitrite as N","INF-1" ),
                    ( "Nitrate Nitrite as N","EFA-2" ),
                    ( "Nitrite as N", "INF-1"),
                    ( "Nitrite as N","EFA-2" )
                },
                RequiredSiteCount = 3,
                RequiredParameterCount = 10
            };
        }
    }
}
```

- Business\RenameEngine.cs

```cs
using JNOT.FileRenamer.ExcelInterop;
using JNOT.FileRenamer.FileSystem;
using System;
using System.IO;
using System.Linq;

namespace JNOT.FileRenamer.Business
{
    public class RenameEngine
    {
        private readonly PatternEngine _patternEngine;
        private readonly SafeRenameService _renameService;

        public RenameEngine(PatternEngine patternEngine, SafeRenameService renameService)
        {
            _patternEngine = patternEngine;
            _renameService = renameService;
        }

        // ---------------------------------------------------------
        // EXCEL FINAL NAME
        // ---------------------------------------------------------
        public string BuildFinalName(PivotData data)
        {
            DateTime dt = DateTime.Parse(data.SampleDateRaw);
            string type = _patternEngine.ResolveTypeCode(data);

            return $"{dt:yyyy-MM-dd} ({type}) Lab Report EF WWTP1.xlsx";
        }

        // ---------------------------------------------------------
        // MAIN RENAME ENTRY POINT (NOW WITH DRY RUN)
        // ---------------------------------------------------------
        public void Rename(
            string sourcePath,
            string destPath,
            PivotData data,
            string jobNumber,
            string typeCode,
            string pdfInputFolder,
            string pdfOutputFolder,
            bool dryRun)
        {
            // Rename Excel file first
            _renameService.Rename(sourcePath, destPath, dryRun);

            // Rename PDF in input folder
            string? renamedPdf = RenamePdfIfExists(pdfInputFolder, data, jobNumber, typeCode, dryRun);

            // If a PDF was renamed, move it to the output folder
            if (renamedPdf != null)
            {
                string finalName = Path.GetFileName(renamedPdf);
                string finalDest = Path.Combine(pdfOutputFolder, finalName);

                _renameService.Rename(renamedPdf, finalDest, dryRun);
            }
        }

        // ---------------------------------------------------------
        // PDF FINAL NAME
        // ---------------------------------------------------------
        public string BuildFinalPdfName(PivotData data, string jobNumber, string typeCode)
        {
            DateTime dt = DateTime.Parse(data.SampleDateRaw);

            string descriptor = typeCode switch
            {
                "W" => "Weekly",
                "M" => "Monthly",
                "S" or "D" => dt.ToString("ddd"),
                _ => "Unknown"
            };

            var parts = jobNumber.Split('-');
            string jobCode = parts.Length > 1 ? parts[1] : "";
            string sampleIndex = parts.Length > 2 ? parts[2] : "";
            string pdfJob = $"J{jobCode}-{sampleIndex}";

            return $"{dt:yyyy-MM-dd} Lab EF {descriptor} {pdfJob}.pdf";
        }

        // ---------------------------------------------------------
        // PDF RENAME LOGIC (NOW WITH DRY RUN)
        // ---------------------------------------------------------
        public string? RenamePdfIfExists(
            string folder,
            PivotData data,
            string jobNumber,
            string typeCode,
            bool dryRun)
        {
            if (string.IsNullOrWhiteSpace(folder) ||
                string.IsNullOrWhiteSpace(jobNumber))
                return null;

            var parts = jobNumber.Split('-');
            if (parts.Length < 3)
                return null;

            string jobCode = parts[1];
            string sampleIndex = parts[2];
            string pdfKey = $"{jobCode}-{sampleIndex}";

            var pdfFiles = Directory.GetFiles(folder, "*.pdf", SearchOption.TopDirectoryOnly);

            string? match = pdfFiles
                .FirstOrDefault(f =>
                {
                    string fileName = Path.GetFileName(f) ?? string.Empty;
                    return fileName.IndexOf(pdfKey, StringComparison.OrdinalIgnoreCase) >= 0;
                });

            if (match == null)
                return null;

            string finalPdfName = BuildFinalPdfName(data, jobNumber, typeCode);
            string destPath = Path.Combine(folder, finalPdfName);

            _renameService.Rename(match, destPath, dryRun);

            return destPath;
        }
    }
}
```


## 📁 Directory: Config

- Config\ConfigPaneProvider.cs

```cs
using JNOT.FileRenamer.Config;
using JNOT.Shared.Config.IO;
using JNOT.Shared.Config.Migration;
using JNOT.Shared.Config.Models;
using JNOT.Shared.Config.Services;
using JNOT.Shared.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

public class ConfigPaneProvider : ITaskPaneContentProvider
{
    private readonly IFileRenamerConfigAdapter _adapter;
    private FileRenamerConfig _cfg;

    public string Title => "File Renamer Configuration";
    public string Status => "Edit settings and save";

    public ConfigPaneProvider(IFileRenamerConfigAdapter adapter)
    {
        _adapter = adapter;
        _cfg = adapter.GetConfig();
    }

    public Task PopulateAsync(Panel panel)
    {
        // No async needed — UI must be built on UI thread anyway
        if (panel.InvokeRequired)
        {
            System.Diagnostics.Debug.WriteLine("PopulateAsync: InvokeRequired");
            panel.BeginInvoke(new Action(() => BuildUI(panel)));
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("PopulateAsync: NOT InvokeRequired");
            BuildUI(panel);
        }

        return Task.CompletedTask;
    }

    private void BuildUI(Panel panel)
    {
        panel.Controls.Clear();

        var lblInput = new Label { Text = "Input Folder:", AutoSize = true, Top = 5, Left = 10 };
        var txtInput = new TextBox { Text = _cfg.InputFolder, Width = 260, Top = 30, Left = 10 };
        var btnInput = new Button { Text = "...", Width = 30, Top = 30, Left = 280 };

        btnInput.Click += (s, e) =>
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtInput.Text = dlg.SelectedPath;
        };

        var lblOutput = new Label { Text = "Output Folder:", AutoSize = true, Top = 65, Left = 10 };
        var txtOutput = new TextBox { Text = _cfg.OutputFolder, Width = 260, Top = 90, Left = 10 };
        var btnOutput = new Button { Text = "...", Width = 30, Top = 90, Left = 280 };

        btnOutput.Click += (s, e) =>
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtOutput.Text = dlg.SelectedPath;
        };

        var chkDebug = new CheckBox
        {
            Text = "Enable Debug Mode",
            Checked = _cfg.Debug,
            Top = 130,
            Left = 10,
            AutoSize = true
        };
        var chkDryRun = new CheckBox
        {
            Text = "Dry Run (simulate only)",
            Checked = _cfg.DryRun,
            Top = 160,
            Left = 10,
            AutoSize = true
        };

        var btnSave = new Button
        {
            Text = "Save Configuration",
            Width = 150,
            Top = 200,
            Left = 10
        };

        btnSave.Click += (s, e) =>
        {
            try
            {
                _cfg.InputFolder = txtInput.Text;
                _cfg.OutputFolder = txtOutput.Text;
                _cfg.Debug = chkDebug.Checked;
                _cfg.DryRun = chkDryRun.Checked;

                var svc = new ConfigService(
                    new ConfigLoader(new ConfigMigrationEngine()),
                    new ConfigWriter()
                );

                svc.Save(new RootConfig
                {
                    FileRenamer = _cfg,
                    Title = "JNOT Global Configuration",
                    Version = "1.0"
                });

                MessageBox.Show("Configuration saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving configuration:\n" + ex.Message);
            }
        };

        panel.Controls.Add(lblInput);
        panel.Controls.Add(txtInput);
        panel.Controls.Add(btnInput);

        panel.Controls.Add(lblOutput);
        panel.Controls.Add(txtOutput);
        panel.Controls.Add(btnOutput);

        panel.Controls.Add(chkDebug);
        panel.Controls.Add(chkDryRun);
        panel.Controls.Add(btnSave);
    }
}
```

- Config\FileRenamerConfigAdapter.cs

```cs
using JNOT.Shared.Config.Models;
using JNOT.Shared.Config.Services;

namespace JNOT.FileRenamer.Config;

public class FileRenamerConfigAdapter : IFileRenamerConfigAdapter
{
    private readonly IConfigService _configService;

    public FileRenamerConfigAdapter(IConfigService configService)
    {
        _configService = configService;
    }

    public FileRenamerConfig GetConfig()
    {
        var root = _configService.LoadOrCreate();
        return root.FileRenamer;
    }
}
```

- Config\IFileRenamerConfigAdapter.cs

```cs
using JNOT.Shared.Config.Models;

namespace JNOT.FileRenamer.Config;

public interface IFileRenamerConfigAdapter
{
    FileRenamerConfig GetConfig();
}
```


## 📁 Directory: ExcelInterop

- ExcelInterop\ExcelReader.cs

```cs
using System;
using Microsoft.Office.Interop.Excel;

namespace JNOT.FileRenamer.ExcelInterop
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

- ExcelInterop\HeaderBlock.cs

```cs

using System;
using System.Collections.Generic;

namespace JNOT.FileRenamer.ExcelInterop
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

- ExcelInterop\ParameterBlock.cs

```cs
using System;
using System.Collections.Generic;

namespace JNOT.FileRenamer.ExcelInterop
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

- ExcelInterop\ParamSitePair.cs

```cs
namespace JNOT.FileRenamer.ExcelInterop
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

- ExcelInterop\PivotData.cs

```cs
using System.Collections.Generic;

namespace JNOT.FileRenamer.ExcelInterop
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

- ExcelInterop\PivotParser.cs

```cs
using System;
using System.Collections.Generic;

namespace JNOT.FileRenamer.ExcelInterop
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

- FileRenamer.csproj
- FileRenamer.csproj.user
- FileRenamer.sln
- FileRenamer_TemporaryKey.pfx

## 📁 Directory: FileSystem

- FileSystem\InputFolderScanner.cs

```cs
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
```

- FileSystem\OutputFolderWriter.cs

```cs
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
```

- FileSystem\SafeRenameService.cs

```cs
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
```


## 📁 Directory: Logging

- Logging\Logger.cs

```cs
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
```

- packages.config

## 📁 Directory: Properties

- Properties\AssemblyInfo.cs

```cs
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("FileRenamer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("FileRenamer")]
[assembly: AssemblyCopyright("Copyright ©  2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("8a2d2c22-9635-4313-b280-efee9a6369ca")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]


```

- Properties\Resources.Designer.cs

```cs
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FileRenamer.Properties {
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("FileRenamer.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
    }
}

```

- Properties\Resources.resx
- Properties\Settings.Designer.cs

```cs
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FileRenamer.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "18.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
    }
}

```

- Properties\Settings.settings
- Ribbon.xml
- RibbonMain.cs

```cs
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Office = Microsoft.Office.Core;

namespace JNOT.FileRenamer
{
    [ComVisible(true)]
    public class RibbonMain : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI _ribbon;

        public RibbonMain()
        {
        }

        public string GetCustomUI(string ribbonID)
        {
            // Embedded resource name MUST match the actual namespace + folder + filename
            return LoadRibbonXml("JNOT.FileRenamer.Ribbon.xml");
        }

        private string LoadRibbonXml(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Ribbon XML resource not found: " + resourceName);

                // Dump available resources to Output window
                foreach (var name in asm.GetManifestResourceNames())
                    System.Diagnostics.Debug.WriteLine("RESOURCE: " + name);

                return "<customUI xmlns='http://schemas.microsoft.com/office/2009/07/customui'></customUI>";
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            _ribbon = ribbonUI;
        }

        public void OpenFileRenamer_Click(Office.IRibbonControl control)
        {
            System.Diagnostics.Debug.WriteLine("OpenFileRenamer_Click fired");
            Globals.ThisAddIn.ShowFileRenamerPane();
        }

        public void OpenConfig_Click(Office.IRibbonControl control)
        {
            System.Diagnostics.Debug.WriteLine("OpenConfig_Click fired");
            Globals.ThisAddIn.ShowConfigPane();
        }
    }
}
```

- ThisAddIn.cs

```cs
using JNOT.FileRenamer.Config;
using JNOT.FileRenamer.Logging;
using JNOT.FileRenamer.UI;
using JNOT.Shared.Config.IO;
using JNOT.Shared.Config.Migration;
using JNOT.Shared.Config.Services;
using JNOT.Shared.UI;
using Microsoft.Office.Tools.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;


namespace JNOT.FileRenamer
{
    
    public partial class ThisAddIn
    {
        private Microsoft.Office.Tools.CustomTaskPane _ctp;
        private Microsoft.Office.Tools.CustomTaskPane _configPane;
        private IFileRenamerConfigAdapter _adapter;
        private Logger _logger;
        private JnotTaskPane _pane;

        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new RibbonMain();
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            var configService = new ConfigService(
                new ConfigLoader(new ConfigMigrationEngine()),
                new ConfigWriter()
            );

            _adapter = new FileRenamerConfigAdapter(configService);
            _logger = new Logger(_adapter.GetConfig().OutputFolder);
            var provider = new FileRenamerPaneProvider(_adapter, _logger);

            var paneControl = new JnotTaskPane();

            // Load the provider into the pane
            // (fire and forget is fine here because PopulateAsync is UI-safe)
            _ = paneControl.LoadFromAsync(provider);

            // Add to VSTO task panes
            Globals.ThisAddIn.CustomTaskPanes.Add(
                paneControl,
                "File Renamer"
            );
        }

        public async void ShowFileRenamerPane()
        {
            var provider = new FileRenamerPaneProvider(_adapter, _logger);

            if (_ctp == null)
            {
                _pane = new JnotTaskPane();
                await _pane.LoadFromAsync(provider);

                _ctp = this.CustomTaskPanes.Add(_pane, "File Renamer");
                _ctp.Width = 400;
            }
            else
            {
                // Refresh the pane with updated config
                await _pane.LoadFromAsync(provider);
            }

            _ctp.Visible = true;
        }

        public void ShowConfigPane()
        {
            if (_configPane == null)
            {
                var pane = new JnotTaskPane();
                var provider = new ConfigPaneProvider(
                    new FileRenamerConfigAdapter(
                        new ConfigService(
                            new ConfigLoader(new ConfigMigrationEngine()),
                            new ConfigWriter()
                        )
                    )
                );

                _configPane = this.CustomTaskPanes.Add(pane, "Configuration");
                _configPane.Width = 420;

                // Load content asynchronously
                _ = pane.LoadFromAsync(provider);
            }

            _configPane.Visible = true;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}

```

- ThisAddIn.Designer.cs

```cs
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#pragma warning disable 414
namespace JNOT.FileRenamer {
    
    /// 
    [Microsoft.VisualStudio.Tools.Applications.Runtime.StartupObjectAttribute(0)]
    [global::System.Security.Permissions.PermissionSetAttribute(global::System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
    public sealed partial class ThisAddIn : Microsoft.Office.Tools.AddInBase {
        
        internal Microsoft.Office.Tools.CustomTaskPaneCollection CustomTaskPanes;
        
        internal Microsoft.Office.Tools.SmartTagCollection VstoSmartTags;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        private global::System.Object missing = global::System.Type.Missing;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        internal Microsoft.Office.Interop.Excel.Application Application;
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        public ThisAddIn(global::Microsoft.Office.Tools.Excel.ApplicationFactory factory, global::System.IServiceProvider serviceProvider) : 
                base(factory, serviceProvider, "AddIn", "ThisAddIn") {
            Globals.Factory = factory;
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        protected override void Initialize() {
            base.Initialize();
            this.Application = this.GetHostItem<Microsoft.Office.Interop.Excel.Application>(typeof(Microsoft.Office.Interop.Excel.Application), "Application");
            Globals.ThisAddIn = this;
            global::System.Windows.Forms.Application.EnableVisualStyles();
            this.InitializeCachedData();
            this.InitializeControls();
            this.InitializeComponents();
            this.InitializeData();
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        protected override void FinishInitialization() {
            this.InternalStartup();
            this.OnStartup();
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        protected override void InitializeDataBindings() {
            this.BeginInitialization();
            this.BindToData();
            this.EndInitialization();
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        private void InitializeCachedData() {
            if ((this.DataHost == null)) {
                return;
            }
            if (this.DataHost.IsCacheInitialized) {
                this.DataHost.FillCachedData(this);
            }
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        private void InitializeData() {
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        private void BindToData() {
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private void StartCaching(string MemberName) {
            this.DataHost.StartCaching(this, MemberName);
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private void StopCaching(string MemberName) {
            this.DataHost.StopCaching(this, MemberName);
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private bool IsCached(string MemberName) {
            return this.DataHost.IsCached(this, MemberName);
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        private void BeginInitialization() {
            this.BeginInit();
            this.CustomTaskPanes.BeginInit();
            this.VstoSmartTags.BeginInit();
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        private void EndInitialization() {
            this.VstoSmartTags.EndInit();
            this.CustomTaskPanes.EndInit();
            this.EndInit();
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        private void InitializeControls() {
            this.CustomTaskPanes = Globals.Factory.CreateCustomTaskPaneCollection(null, null, "CustomTaskPanes", "CustomTaskPanes", this);
            this.VstoSmartTags = Globals.Factory.CreateSmartTagCollection(null, null, "VstoSmartTags", "VstoSmartTags", this);
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        private void InitializeComponents() {
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private bool NeedsFill(string MemberName) {
            return this.DataHost.NeedsFill(this, MemberName);
        }
        
        /// 
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        protected override void OnShutdown() {
            this.VstoSmartTags.Dispose();
            this.CustomTaskPanes.Dispose();
            base.OnShutdown();
        }
    }
    
    /// 
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
    internal sealed partial class Globals {
        
        /// 
        private Globals() {
        }
        
        private static ThisAddIn _ThisAddIn;
        
        private static global::Microsoft.Office.Tools.Excel.ApplicationFactory _factory;
        
        private static ThisRibbonCollection _ThisRibbonCollection;
        
        internal static ThisAddIn ThisAddIn {
            get {
                return _ThisAddIn;
            }
            set {
                if ((_ThisAddIn == null)) {
                    _ThisAddIn = value;
                }
                else {
                    throw new System.NotSupportedException();
                }
            }
        }
        
        internal static global::Microsoft.Office.Tools.Excel.ApplicationFactory Factory {
            get {
                return _factory;
            }
            set {
                if ((_factory == null)) {
                    _factory = value;
                }
                else {
                    throw new System.NotSupportedException();
                }
            }
        }
        
        internal static ThisRibbonCollection Ribbons {
            get {
                if ((_ThisRibbonCollection == null)) {
                    _ThisRibbonCollection = new ThisRibbonCollection(_factory.GetRibbonFactory());
                }
                return _ThisRibbonCollection;
            }
        }
    }
    
    /// 
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Tools.Office.ProgrammingModel.dll", "18.0.0.0")]
    internal sealed partial class ThisRibbonCollection : Microsoft.Office.Tools.Ribbon.RibbonCollectionBase {
        
        /// 
        internal ThisRibbonCollection(global::Microsoft.Office.Tools.Ribbon.RibbonFactory factory) : 
                base(factory) {
        }
    }
}

```

- ThisAddIn.Designer.xml

## 📁 Directory: UI

- UI\FileRenamerPaneProvider.cs

```cs
using JNOT.FileRenamer.Business;
using JNOT.FileRenamer.Config;
using JNOT.FileRenamer.ExcelInterop;
using JNOT.FileRenamer.FileSystem;
using JNOT.FileRenamer.Logging;
using JNOT.Shared.Config.Models;
using JNOT.Shared.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JNOT.FileRenamer.UI
{
    public class FileRenamerPaneProvider : ITaskPaneContentProvider
    {
        private readonly IFileRenamerConfigAdapter _configAdapter;
        private readonly Logger _logger;

        public string Title => "File Renamer";
        public string Status => "Ready to rename files";

        public FileRenamerPaneProvider(IFileRenamerConfigAdapter configAdapter, Logger logger)
        {
            _configAdapter = configAdapter;
            _logger = logger;
        }

        public Task PopulateAsync(Panel panel)
        {
            if (panel.InvokeRequired)
            {
                panel.BeginInvoke(new Action(() => BuildUI(panel)));
            }
            else
            {
                BuildUI(panel);
            }

            return Task.CompletedTask;
        }

        private void BuildUI(Panel panel)
        {
            panel.Controls.Clear();

            var cfg = _configAdapter.GetConfig();

            // -----------------------------
            // Input Folder (read-only)
            // -----------------------------
            var lblInput = new Label
            {
                Text = "Input Folder:",
                AutoSize = true,
                Top = 10,
                Left = 10
            };

            var txtInput = new TextBox
            {
                Text = cfg.InputFolder,
                Width = 350,
                Top = 30,
                Left = 10,
                ReadOnly = true
            };

            // -----------------------------
            // Output Folder (read-only)
            // -----------------------------
            var lblOutput = new Label
            {
                Text = "Output Folder:",
                AutoSize = true,
                Top = 70,
                Left = 10
            };

            var txtOutput = new TextBox
            {
                Text = cfg.OutputFolder,
                Width = 350,
                Top = 90,
                Left = 10,
                ReadOnly = true
            };

            // -----------------------------
            // Run Button
            // -----------------------------
            var btnRun = new Button
            {
                Text = "Run",
                Width = 120,
                Top = 130,
                Left = 10
            };

            // -----------------------------
            // Log Output
            // -----------------------------
            var txtLog = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Width = 450,
                Height = 300,
                Top = 170,
                Left = 10
            };

            // -----------------------------
            // Run Button Click Handler
            // -----------------------------
            btnRun.Click += (s, e) =>
            {
                btnRun.Enabled = false;
                txtLog.Clear();

                try
                {
                    RunRenamePipeline(cfg, txtLog);
                }
                catch (Exception ex)
                {
                    txtLog.AppendText($"ERROR: {ex.Message}{Environment.NewLine}");
                    _logger.Log($"ERROR: {ex.Message}");
                }
                finally
                {
                    btnRun.Enabled = true;
                }
            };

            // -----------------------------
            // Add Controls
            // -----------------------------
            panel.Controls.Add(lblInput);
            panel.Controls.Add(txtInput);

            panel.Controls.Add(lblOutput);
            panel.Controls.Add(txtOutput);

            panel.Controls.Add(btnRun);
            panel.Controls.Add(txtLog);
        }
        private void RunRenamePipeline(FileRenamerConfig cfg, TextBox log)
        {
            var scanner = new InputFolderScanner();
            var excelReader = new ExcelReader();
            var patternEngine = new PatternEngine();
            var renameService = new SafeRenameService();
            var renameEngine = new RenameEngine(patternEngine, renameService);
            var outputWriter = new OutputFolderWriter();

            var files = scanner.Scan(cfg.InputFolder);

            if (!files.Any())
            {
                log.AppendText("No .xlsx files found in input folder." + Environment.NewLine);
                _logger.Log("No .xlsx files found in input folder.");
                return;
            }

            // ---------------------------------------------------------
            // DRY RUN BANNER
            // ---------------------------------------------------------
            if (cfg.DryRun)
            {
                log.AppendText("DRY RUN MODE — No files will be modified." + Environment.NewLine);
                _logger.Log("DRY RUN MODE — No files will be modified.");
            }

            log.AppendText($"Found {files.Count} file(s). Starting rename..." + Environment.NewLine);
            _logger.Log($"Found {files.Count} file(s). Starting rename...");

            int index = 0;
            foreach (var file in files)
            {
                index++;
                string header = $"[{index}/{files.Count}] Processing: {file}";
                log.AppendText(header + Environment.NewLine);
                _logger.Log(header);

                try
                {
                    var pivot = excelReader.ReadPivot(file);
                    var typeCode = patternEngine.ResolveTypeCode(pivot);

                    string finalExcelName = renameEngine.BuildFinalName(pivot);
                    string destExcelPath = outputWriter.BuildOutputPath(cfg.OutputFolder, finalExcelName);

                    // ---------------------------------------------------------
                    // DRY RUN — Excel rename simulation
                    // ---------------------------------------------------------
                    if (cfg.DryRun)
                    {
                        string msg = $"DRY RUN → Would rename: {file} → {destExcelPath}";
                        log.AppendText(msg + Environment.NewLine);
                        _logger.Log(msg);
                    }

                    // ---------------------------------------------------------
                    // Perform rename (or simulate, depending on DryRun)
                    // ---------------------------------------------------------
                    renameEngine.Rename(
                        sourcePath: file,
                        destPath: destExcelPath,
                        data: pivot,
                        jobNumber: pivot.JobNumberRaw,
                        typeCode: typeCode,
                        pdfInputFolder: cfg.InputFolder,
                        pdfOutputFolder: cfg.OutputFolder,
                        dryRun: cfg.DryRun
                    );

                    // ---------------------------------------------------------
                    // DRY RUN — PDF rename simulation
                    // ---------------------------------------------------------
                    if (cfg.DryRun)
                    {
                        string pdfKey = pivot.JobNumberRaw;
                        string msg = $"DRY RUN → Would rename matching PDF for job {pdfKey} (if found)";
                        log.AppendText(msg + Environment.NewLine);
                        _logger.Log(msg);
                    }

                    // ---------------------------------------------------------
                    // Real-run success message
                    // ---------------------------------------------------------
                    if (!cfg.DryRun)
                    {
                        string successMsg = $"SUCCESS → {finalExcelName}";
                        log.AppendText(successMsg + Environment.NewLine);
                        _logger.Log(successMsg);
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = $"ERROR → {ex.Message}";
                    log.AppendText(errorMsg + Environment.NewLine);
                    _logger.Log(errorMsg);
                }

                log.AppendText(Environment.NewLine);
            }

            log.AppendText("Rename operation completed." + Environment.NewLine);
            _logger.Log("Rename operation completed.");
        }
        private void oldRunRenamePipeline(FileRenamerConfig cfg, TextBox log)
        {
            var scanner = new InputFolderScanner();
            var excelReader = new ExcelReader();
            var patternEngine = new PatternEngine();
            var renameService = new SafeRenameService();
            var renameEngine = new RenameEngine(patternEngine, renameService);
            var outputWriter = new OutputFolderWriter();

            var files = scanner.Scan(cfg.InputFolder);

            if (!files.Any())
            {
                log.AppendText("No .xlsx files found in input folder." + Environment.NewLine);
                _logger.Log("No .xlsx files found in input folder.");
                return;
            }

            log.AppendText($"Found {files.Count} file(s). Starting rename..." + Environment.NewLine);
            _logger.Log($"Found {files.Count} file(s). Starting rename...");

            int index = 0;
            foreach (var file in files)
            {
                index++;
                string header = $"[{index}/{files.Count}] Processing: {file}";
                log.AppendText(header + Environment.NewLine);
                _logger.Log(header);

                try
                {
                    var pivot = excelReader.ReadPivot(file);
                    var typeCode = patternEngine.ResolveTypeCode(pivot);

                    string finalExcelName = renameEngine.BuildFinalName(pivot);
                    string destExcelPath = outputWriter.BuildOutputPath(cfg.OutputFolder, finalExcelName);

                    renameEngine.Rename(
                        sourcePath: file,
                        destPath: destExcelPath,
                        data: pivot,
                        jobNumber: pivot.JobNumberRaw,
                        typeCode: typeCode,
                        pdfInputFolder: cfg.InputFolder,
                        pdfOutputFolder: cfg.OutputFolder,
                        dryRun: cfg.DryRun
                    );

                    string successMsg = $"SUCCESS → {finalExcelName}";
                    log.AppendText(successMsg + Environment.NewLine);
                    _logger.Log(successMsg);
                }
                catch (Exception ex)
                {
                    string errorMsg = $"ERROR → {ex.Message}";
                    log.AppendText(errorMsg + Environment.NewLine);
                    _logger.Log(errorMsg);
                }

                log.AppendText(Environment.NewLine);
            }

            log.AppendText("Rename operation completed." + Environment.NewLine);
            _logger.Log("Rename operation completed.");
        }
    }
}
```

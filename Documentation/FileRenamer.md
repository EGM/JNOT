
## 📁 Directory: /


## 📁 Directory: Business

- Business\Pattern.cs

```cs
using System.Collections.Generic;

namespace Jnot.FileRenamer.Business
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
using Jnot.Excel.Interop;
using System.Linq;
using Tomlyn;

namespace Jnot.FileRenamer.Business
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
using Jnot.Excel.Interop;
using Jnot.Shared.FileSystem;
using System;
using System.IO;
using System.Linq;

namespace Jnot.FileRenamer.Business
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

- Class1.cs

```cs
namespace FileRenamer
{
    public class Class1
    {

    }
}

```


## 📁 Directory: Config

- Config\ConfigPaneProvider.cs

```cs
using Jnot.FileRenamer.Config;
using Jnot.Shared.Config.IO;
using Jnot.Shared.Config.Migration;
using Jnot.Shared.Config.Models;
using Jnot.Shared.Config.Services;
using Jnot.Shared.UI;
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
        var chkClearOnRun = new CheckBox
        {
            Text = "Clear On Run",
            Checked = _cfg.ClearOnRun,
            Top = 190,
            Left = 10,
            AutoSize = true
        };

        var btnSave = new Button
        {
            Text = "Save Configuration",
            Width = 150,
            Top = 230,
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
                _cfg.ClearOnRun = chkClearOnRun.Checked;

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
        panel.Controls.Add(chkClearOnRun);
        panel.Controls.Add(btnSave);
    }
}

```

- Config\FileRenamerConfigAdapter.cs

```cs
using Jnot.Shared.Config.Models;
using Jnot.Shared.Config.Services;

namespace Jnot.FileRenamer.Config;

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
using Jnot.Shared.Config.Models;

namespace Jnot.FileRenamer.Config;

public interface IFileRenamerConfigAdapter
{
    FileRenamerConfig GetConfig();
}

```

- FileRenamer.csproj

## 📁 Directory: UI

- UI\FileRenamerPaneProvider.cs

```cs
using Jnot.FileRenamer.Business;
using Jnot.FileRenamer.Config;
using Jnot.Excel.Interop;
using Jnot.FileRenamer.FileSystem;
using Jnot.FileRenamer.Logging;
using Jnot.Shared.Config.Models;
using Jnot.Shared.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jnot.Shared.UI.Panels;
using Jnot.Shared.UI.Controls;

namespace Jnot.FileRenamer.UI
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
            var txtLog = new JnotRichOutputBox
            {
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.ForcedVertical,
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
                    txtLog.AppendError($"ERROR: {ex.Message}{Environment.NewLine}");
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
        private void RunRenamePipeline(FileRenamerConfig cfg, JnotRichOutputBox log)
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
                log.AppendWarning("No .xlsx files found in input folder." + Environment.NewLine);
                _logger.Log("No .xlsx files found in input folder.");
                return;
            }

            // ---------------------------------------------------------
            // DRY RUN BANNER
            // ---------------------------------------------------------
            if (cfg.DryRun)
            {
                log.AppendDebug("DRY RUN MODE — No files will be modified." + Environment.NewLine);
                _logger.Log("DRY RUN MODE — No files will be modified.");
            }

            log.AppendInfo($"Found {files.Count} file(s). Starting rename..." + Environment.NewLine);
            _logger.Log($"Found {files.Count} file(s). Starting rename...");

            int index = 0;
            foreach (var file in files)
            {
                index++;
                string header = $"[{index}/{files.Count}] Processing: {file}";
                log.AppendInfo(header + Environment.NewLine);
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
                        log.AppendDebug(msg + Environment.NewLine);
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
                        log.AppendDebug(msg + Environment.NewLine);
                        _logger.Log(msg);
                    }

                    // ---------------------------------------------------------
                    // Real-run success message
                    // ---------------------------------------------------------
                    if (!cfg.DryRun)
                    {
                        string successMsg = $"SUCCESS → {finalExcelName}";
                        log.AppendSuccess(successMsg + Environment.NewLine);
                        _logger.Log(successMsg);
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = $"ERROR → {ex.Message}";
                    log.AppendError(errorMsg + Environment.NewLine);
                    _logger.Log(errorMsg);
                }

                log.AppendInfo(Environment.NewLine);
            }

            log.AppendInfo("Rename operation completed." + Environment.NewLine);
            _logger.Log("Rename operation completed.");
        }
        private void oldRunRenamePipeline(FileRenamerConfig cfg, JnotRichOutputBox log)
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
                log.AppendWarning("No .xlsx files found in input folder." + Environment.NewLine);
                _logger.Log("No .xlsx files found in input folder.");
                return;
            }

            log.AppendInfo($"Found {files.Count} file(s). Starting rename..." + Environment.NewLine);
            _logger.Log($"Found {files.Count} file(s). Starting rename...");

            int index = 0;
            foreach (var file in files)
            {
                index++;
                string header = $"[{index}/{files.Count}] Processing: {file}";
                log.AppendInfo(header + Environment.NewLine);
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
                    log.AppendSuccess(successMsg + Environment.NewLine);
                    _logger.Log(successMsg);
                }
                catch (Exception ex)
                {
                    string errorMsg = $"ERROR → {ex.Message}";
                    log.AppendError(errorMsg + Environment.NewLine);
                    _logger.Log(errorMsg);
                }

                log.AppendInfo(Environment.NewLine);
            }

            log.AppendInfo("Rename operation completed." + Environment.NewLine);
            _logger.Log("Rename operation completed.");
        }
    }
}

```

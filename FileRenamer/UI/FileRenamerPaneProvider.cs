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
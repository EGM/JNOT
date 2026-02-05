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
        // MAIN RENAME ENTRY POINT
        // ---------------------------------------------------------
        public void Rename(
            string sourcePath,
            string destPath,
            PivotData data,
            string jobNumber,
            string typeCode,
            string pdfInputFolder,
            string pdfOutputFolder)
        {
            // Rename Excel file first
            _renameService.Rename(sourcePath, destPath);

            // Rename PDF in input folder
            string? renamedPdf = RenamePdfIfExists(pdfInputFolder, data, jobNumber, typeCode);

            // If a PDF was renamed, move it to the output folder
            if (renamedPdf != null)
            {
                string finalName = Path.GetFileName(renamedPdf);
                string finalDest = Path.Combine(pdfOutputFolder, finalName);

                _renameService.Rename(renamedPdf, finalDest);
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
        // PDF RENAME LOGIC
        // ---------------------------------------------------------
        public string? RenamePdfIfExists(string folder, PivotData data, string jobNumber, string typeCode)
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
                    //return fileName.Contains(pdfKey, StringComparison.OrdinalIgnoreCase);
                    return fileName.IndexOf(pdfKey, StringComparison.OrdinalIgnoreCase) >= 0;

                });

            if (match == null)
                return null;

            string finalPdfName = BuildFinalPdfName(data, jobNumber, typeCode);
            string destPath = Path.Combine(folder, finalPdfName);

            _renameService.Rename(match, destPath);

            return destPath;
        }
    }
}
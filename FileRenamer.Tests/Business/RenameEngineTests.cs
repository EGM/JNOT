using JNOT.FileRenamer.Business;
using JNOT.FileRenamer.ExcelInterop;
using JNOT.FileRenamer.FileSystem;
using System;
using System.IO;

namespace JNOT.FileRenamer.Tests.Business
{
    public class RenameEngineTests
    {
        [Fact]
        public void PdfName_ShouldMatch_DailyPattern()
        {
            var data = new PivotData
            {
                SampleDateRaw = "2026-01-06", // Tuesday
            };

            var engine = new RenameEngine(
                new PatternEngine(),
                new SafeRenameService()
            );

            var pdfName = engine.BuildFinalPdfName(data, "762-8123-1", "D");

            Assert.Equal("2026-01-06 Lab EF Tue J8123-1.pdf", pdfName);
        }

        [Fact]
        public void PdfName_ShouldMatch_WeekendPattern()
        {
            var data = new PivotData
            {
                SampleDateRaw = "2026-01-04", // Sunday
            };

            var engine = new RenameEngine(
                new PatternEngine(),
                new SafeRenameService()
            );

            var pdfName = engine.BuildFinalPdfName(data, "762-8172-1", "S");

            Assert.Equal("2026-01-04 Lab EF Sun J8172-1.pdf", pdfName);
        }

        [Fact]
        public void PdfName_ShouldMatch_WeeklyPattern()
        {
            var data = new PivotData
            {
                SampleDateRaw = "2026-01-22",
            };

            var engine = new RenameEngine(
                new PatternEngine(),
                new SafeRenameService()
            );

            var pdfName = engine.BuildFinalPdfName(data, "762-7935-2", "W");

            Assert.Equal("2026-01-22 Lab EF Weekly J7935-2.pdf", pdfName);
        }

        [Fact]
        public void PdfName_ShouldMatch_MonthlyPattern()
        {
            var data = new PivotData
            {
                SampleDateRaw = "2026-01-31",
            };

            var engine = new RenameEngine(
                new PatternEngine(),
                new SafeRenameService()
            );

            var pdfName = engine.BuildFinalPdfName(data, "762-75419-1", "M");

            Assert.Equal("2026-01-31 Lab EF Monthly J75419-1.pdf", pdfName);
        }

        [Fact]
        public void PdfName_ShouldExtractJobNumberCorrectly()
        {
            var engine = new RenameEngine(
                new PatternEngine(),
                new SafeRenameService()
            );

            var pdfName = engine.BuildFinalPdfName(
                new PivotData { SampleDateRaw = "2026-01-06" },
                "762-8123-1",
                "D"
            );

            Assert.Contains("8123-1.pdf", pdfName);
        }

        [Fact]
        public void RenamePdfIfExists_ShouldRenameMatchingPdf()
        {
            using var temp = new TempFolder();

            var folder = temp.Path;

            File.WriteAllText(Path.Combine(folder, "J8172-1 UDS Level 2 Report.pdf"), "dummy");

            var engine = new RenameEngine(new PatternEngine(), new SafeRenameService());

            var data = new PivotData { SampleDateRaw = "2026-01-04" };

            engine.RenamePdfIfExists(folder, data, "762-8172-1", "S", dryRun: false);

            Assert.True(File.Exists(Path.Combine(folder, "2026-01-04 Lab EF Sun J8172-1.pdf")));
        }

        [Fact]
        public void RenamePdfIfExists_ShouldOnlyRenameMatchingPdf()
        {
            using var temp = new TempFolder();

            var folder = temp.Path;

            File.WriteAllText(Path.Combine(folder, "J7935-2.pdf"), "dummy");
            File.WriteAllText(Path.Combine(folder, "J1111-1.pdf"), "dummy");

            var engine = new RenameEngine(new PatternEngine(), new SafeRenameService());

            var data = new PivotData { SampleDateRaw = "2026-01-22" };

            engine.RenamePdfIfExists(folder, data, "762-7935-2", "W", dryRun: false);

            Assert.True(File.Exists(Path.Combine(folder, "2026-01-22 Lab EF Weekly J7935-2.pdf")));
            Assert.True(File.Exists(Path.Combine(folder, "J1111-1.pdf"))); // untouched
        }

        [Fact]
        public void RenamePdfIfExists_ShouldDoNothing_WhenNoPdfMatches()
        {
            using var temp = new TempFolder();

            var folder = temp.Path;

            // Create a PDF that does NOT match job number 8123-1
            File.WriteAllText(Path.Combine(folder, "J9999-1.pdf"), "dummy");

            var engine = new RenameEngine(new PatternEngine(), new SafeRenameService());

            var data = new PivotData { SampleDateRaw = "2026-01-06" };

            engine.RenamePdfIfExists(folder, data, "762-8123-1", "S", dryRun: false);

            // No file should be renamed
            Assert.True(File.Exists(Path.Combine(folder, "J9999-1.pdf")));
        }
    }
}

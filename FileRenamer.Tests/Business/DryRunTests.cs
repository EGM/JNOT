using JNOT.FileRenamer.Business;
using JNOT.FileRenamer.ExcelInterop;
using JNOT.FileRenamer.FileSystem;
using System.IO;

namespace JNOT.FileRenamer.Tests.Business
{
    public class DryRunTests
    {
        [Fact]
        public void DryRun_ShouldNotRename_ExcelFile()
        {
            using var temp = new TempFolder();

            string input = Path.Combine(temp.Path, "input.xlsx");
            File.WriteAllText(input, "dummy");

            string output = Path.Combine(temp.Path, "output.xlsx");

            var engine = new RenameEngine(new PatternEngine(), new SafeRenameService());

            var data = new PivotData { SampleDateRaw = "2026-01-06" };

            engine.Rename(
                sourcePath: input,
                destPath: output,
                data: data,
                jobNumber: "762-8123-1",
                typeCode: "D",
                pdfInputFolder: temp.Path,
                pdfOutputFolder: temp.Path,
                dryRun: true
            );

            Assert.True(File.Exists(input));      // original still exists
            Assert.False(File.Exists(output));    // destination not created
        }

        [Fact]
        public void DryRun_ShouldNotRename_PdfFile()
        {
            using var temp = new TempFolder();

            string pdf = Path.Combine(temp.Path, "J8123-1.pdf");
            File.WriteAllText(pdf, "dummy");

            var engine = new RenameEngine(new PatternEngine(), new SafeRenameService());

            var data = new PivotData { SampleDateRaw = "2026-01-06" };

            engine.RenamePdfIfExists(
                folder: temp.Path,
                data: data,
                jobNumber: "762-8123-1",
                typeCode: "D",
                dryRun: true
            );

            string expected = Path.Combine(temp.Path, "2026-01-06 Lab EF Tue J8123-1.pdf");

            Assert.True(File.Exists(pdf));        // original still exists
            Assert.False(File.Exists(expected));  // destination not created
        }

        [Fact]
        public void DryRun_ShouldNotDeleteExistingDestination()
        {
            using var temp = new TempFolder();

            string src = Path.Combine(temp.Path, "input.xlsx");
            string dst = Path.Combine(temp.Path, "existing.xlsx");

            File.WriteAllText(src, "source");
            File.WriteAllText(dst, "existing");

            var engine = new RenameEngine(new PatternEngine(), new SafeRenameService());

            var data = new PivotData { SampleDateRaw = "2026-01-06" };

            engine.Rename(
                sourcePath: src,
                destPath: dst,
                data: data,
                jobNumber: "762-8123-1",
                typeCode: "D",
                pdfInputFolder: temp.Path,
                pdfOutputFolder: temp.Path,
                dryRun: true
            );

            Assert.True(File.Exists(src));  // source untouched
            Assert.True(File.Exists(dst));  // destination untouched
        }

        [Fact]
        public void DryRun_ShouldStillMatchPdf_ButNotRename()
        {
            using var temp = new TempFolder();

            string pdf = Path.Combine(temp.Path, "J8172-1.pdf");
            File.WriteAllText(pdf, "dummy");

            var engine = new RenameEngine(new PatternEngine(), new SafeRenameService());

            var data = new PivotData { SampleDateRaw = "2026-01-04" };

            engine.RenamePdfIfExists(
                folder: temp.Path,
                data: data,
                jobNumber: "762-8172-1",
                typeCode: "S",
                dryRun: true
            );

            string expected = Path.Combine(temp.Path, "2026-01-04 Lab EF Sun J8172-1.pdf");

            Assert.True(File.Exists(pdf));        // original still exists
            Assert.False(File.Exists(expected));  // rename not performed
        }

        [Fact]
        public void DryRun_ShouldNotThrow_WhenNoPdfMatches()
        {
            using var temp = new TempFolder();

            File.WriteAllText(Path.Combine(temp.Path, "J9999-1.pdf"), "dummy");

            var engine = new RenameEngine(new PatternEngine(), new SafeRenameService());

            var data = new PivotData { SampleDateRaw = "2026-01-06" };

            var exception = Record.Exception(() =>
                engine.RenamePdfIfExists(
                    folder: temp.Path,
                    data: data,
                    jobNumber: "762-8123-1",
                    typeCode: "D",
                    dryRun: true
                )
            );

            Assert.Null(exception);  // DryRun should never throw
        }
    }
}

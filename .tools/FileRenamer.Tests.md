
## 📁 Directory: /


## 📁 Directory: Business

- Business\DryRunTests.cs

```cs
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
```

- Business\PatternEngineTests.cs

```cs
using JNOT.FileRenamer.Business;
using JNOT.FileRenamer.ExcelInterop;
using JNOT.FileRenamer.FileSystem;
using Xunit;

namespace JNOT.FileRenamer.Tests.Business
{
    public class PatternEngineTests
    {
        private readonly PatternEngine _engine = new();

        private PivotData MakePivot(params (string Parameter, string SiteId)[] pairs)
        {
            return new PivotData
            {
                Pairs = pairs
                    .Select(p => new PivotPair
                    {
                        Parameter = p.Parameter,
                        SiteId = p.SiteId
                    })
                    .ToList()
            };
        }

        [Fact]
        public void WeekendPattern_ShouldMatch_SingleTSS_EFA2()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("S", result);
        }

        [Fact]
        public void DailyPattern_ShouldMatch_ThreeParameters_AllEFA2()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "EFA-2"),
                ("Carbonaceous Biochemical Oxygen Demand", "EFA-2"),
                ("Coliform, Fecal", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("D", result);
        }

        [Fact]
        public void ShouldReturnX_WhenParametersDoNotMatchAnyPattern()
        {
            var data = MakePivot(
                ("pH", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void ShouldReturnX_WhenDailyPatternMissingOneParameter()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "EFA-2"),
                ("Carbonaceous Biochemical Oxygen Demand", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void ShouldReturnX_WhenWeekendPatternHasExtraParameter()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "EFA-2"),
                ("Carbonaceous Biochemical Oxygen Demand", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void ShouldReturnX_WhenDailyPatternHasWrongSite()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "EFA-1"),
                ("Carbonaceous Biochemical Oxygen Demand", "EFA-1"),
                ("Coliform, Fecal", "EFA-1")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void WeeklyPattern_ShouldMatch_AmendedStructure()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "INF-1"),
                ("Total Suspended Solids", "EFA-1"),
                ("Carbonaceous Biochemical Oxygen Demand", "INF-1"),
                ("Coliform, Fecal", "EFA-1 CCC #1"),
                ("Coliform, Fecal", "EFA-1 CCC #2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("W", result);
        }
        [Fact]
        public void WeeklyPattern_ShouldReturnX_WhenMissingARequiredPair()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "INF-1"),
                ("Total Suspended Solids", "EFA-1"),
                ("Carbonaceous Biochemical Oxygen Demand", "INF-1"),
                ("Coliform, Fecal", "EFA-1 CCC #1")
            // missing CCC #2
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void WeeklyPattern_ShouldReturnX_WhenDistinctSiteCountIsWrong()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "INF-1"),
                ("Total Suspended Solids", "INF-1"), // duplicate site
                ("Carbonaceous Biochemical Oxygen Demand", "INF-1"),
                ("Coliform, Fecal", "EFA-1 CCC #1"),
                ("Coliform, Fecal", "EFA-1 CCC #2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void WeeklyPattern_ShouldReturnX_WhenParameterIsAtWrongSite()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "INF-1"),
                ("Total Suspended Solids", "EFA-1"),
                ("Carbonaceous Biochemical Oxygen Demand", "EFA-1"), // wrong site
                ("Coliform, Fecal", "EFA-1 CCC #1"),
                ("Coliform, Fecal", "EFA-1 CCC #2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void WeeklyPattern_ShouldReturnX_WhenExtraParameterPresent()
        {
            var data = MakePivot(
                ("Total Suspended Solids", "INF-1"),
                ("Total Suspended Solids", "EFA-1"),
                ("Carbonaceous Biochemical Oxygen Demand", "INF-1"),
                ("Coliform, Fecal", "EFA-1 CCC #1"),
                ("Coliform, Fecal", "EFA-1 CCC #2"),
                ("pH", "INF-1") // extra parameter
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void WeeklyPattern_ShouldReturnX_WhenSitesAreCorrectButParametersWrong()
        {
            var data = MakePivot(
                ("pH", "INF-1"),
                ("pH", "EFA-1"),
                ("pH", "EFA-1 CCC #1"),
                ("pH", "EFA-1 CCC #2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void MonthlyPattern_ShouldMatch_AmendedStructure()
        {
            var data = MakePivot(
                ("Total Dissolved Solids", "EFA-1"),

                ("Ammonia (as N)", "INF-1"),
                ("Ammonia (as N)", "EFA-2"),

                ("Nitrogen, Kjeldahl", "INF-1"),
                ("Nitrogen, Kjeldahl", "EFA-2"),

                ("Orthophosphate as P, Dissolved", "INF-1"),
                ("Orthophosphate as P, Dissolved", "EFA-2"),

                ("Total Phosphorus as P", "INF-1"),
                ("Total Phosphorus as P", "EFA-2"),

                ("Nitrogen, Organic", "INF-1"),
                ("Nitrogen, Organic", "EFA-2"),

                ("Nitrogen, Total", "INF-1"),
                ("Nitrogen, Total", "EFA-2"),

                ("Nitrate as N", "INF-1"),
                ("Nitrate as N", "EFA-2"),

                ("Nitrate Nitrite as N", "INF-1"),
                ("Nitrate Nitrite as N", "EFA-2"),

                ("Nitrite as N", "INF-1"),
                ("Nitrite as N", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("M", result);
        }

        [Fact]
        public void MonthlyPattern_ShouldReturnX_WhenMissingARequiredPair()
        {
            var data = MakePivot(
                ("Total Dissolved Solids", "EFA-1"),

                ("Ammonia (as N)", "INF-1"),
                ("Ammonia (as N)", "EFA-2"),

                ("Nitrogen, Kjeldahl", "INF-1"),
                ("Nitrogen, Kjeldahl", "EFA-2"),

                ("Orthophosphate as P, Dissolved", "INF-1"),
                ("Orthophosphate as P, Dissolved", "EFA-2"),

                ("Total Phosphorus as P", "INF-1"),
                ("Total Phosphorus as P", "EFA-2"),

                ("Nitrogen, Organic", "INF-1"),
                ("Nitrogen, Organic", "EFA-2"),

                ("Nitrogen, Total", "INF-1"),
                ("Nitrogen, Total", "EFA-2"),

                ("Nitrate as N", "INF-1"),
                ("Nitrate as N", "EFA-2"),

                ("Nitrate Nitrite as N", "INF-1"),
                ("Nitrate Nitrite as N", "EFA-2")

            // Missing ("Nitrite as N", "INF-1") and ("Nitrite as N", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void MonthlyPattern_ShouldReturnX_WhenDistinctSiteCountIsWrong()
        {
            var data = MakePivot(
                ("Total Dissolved Solids", "EFA-1"),

                ("Ammonia (as N)", "INF-1"),
                ("Ammonia (as N)", "INF-1"), // EFA-2 missing

                ("Nitrogen, Kjeldahl", "INF-1"),
                ("Nitrogen, Kjeldahl", "INF-1"), // EFA-2 missing

                ("Orthophosphate as P, Dissolved", "INF-1"),
                ("Orthophosphate as P, Dissolved", "INF-1"),

                ("Total Phosphorus as P", "INF-1"),
                ("Total Phosphorus as P", "INF-1"),

                ("Nitrogen, Organic", "INF-1"),
                ("Nitrogen, Organic", "INF-1"),

                ("Nitrogen, Total", "INF-1"),
                ("Nitrogen, Total", "INF-1"),

                ("Nitrate as N", "INF-1"),
                ("Nitrate as N", "INF-1"),

                ("Nitrate Nitrite as N", "INF-1"),
                ("Nitrate Nitrite as N", "INF-1"),

                ("Nitrite as N", "INF-1"),
                ("Nitrite as N", "INF-1")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void MonthlyPattern_ShouldReturnX_WhenParameterIsAtWrongSite()
        {
            var data = MakePivot(
                ("Total Dissolved Solids", "INF-1"), // wrong site

                ("Ammonia (as N)", "INF-1"),
                ("Ammonia (as N)", "EFA-2"),

                ("Nitrogen, Kjeldahl", "INF-1"),
                ("Nitrogen, Kjeldahl", "EFA-2"),

                ("Orthophosphate as P, Dissolved", "INF-1"),
                ("Orthophosphate as P, Dissolved", "EFA-2"),

                ("Total Phosphorus as P", "INF-1"),
                ("Total Phosphorus as P", "EFA-2"),

                ("Nitrogen, Organic", "INF-1"),
                ("Nitrogen, Organic", "EFA-2"),

                ("Nitrogen, Total", "INF-1"),
                ("Nitrogen, Total", "EFA-2"),

                ("Nitrate as N", "INF-1"),
                ("Nitrate as N", "EFA-2"),

                ("Nitrate Nitrite as N", "INF-1"),
                ("Nitrate Nitrite as N", "EFA-2"),

                ("Nitrite as N", "INF-1"),
                ("Nitrite as N", "EFA-2")
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }

        [Fact]
        public void MonthlyPattern_ShouldReturnX_WhenExtraParameterPresent()
        {
            var data = MakePivot(
                ("Total Dissolved Solids", "EFA-1"),

                ("Ammonia (as N)", "INF-1"),
                ("Ammonia (as N)", "EFA-2"),

                ("Nitrogen, Kjeldahl", "INF-1"),
                ("Nitrogen, Kjeldahl", "EFA-2"),

                ("Orthophosphate as P, Dissolved", "INF-1"),
                ("Orthophosphate as P, Dissolved", "EFA-2"),

                ("Total Phosphorus as P", "INF-1"),
                ("Total Phosphorus as P", "EFA-2"),

                ("Nitrogen, Organic", "INF-1"),
                ("Nitrogen, Organic", "EFA-2"),

                ("Nitrogen, Total", "INF-1"),
                ("Nitrogen, Total", "EFA-2"),

                ("Nitrate as N", "INF-1"),
                ("Nitrate as N", "EFA-2"),

                ("Nitrate Nitrite as N", "INF-1"),
                ("Nitrate Nitrite as N", "EFA-2"),

                ("Nitrite as N", "INF-1"),
                ("Nitrite as N", "EFA-2"),

                ("pH", "INF-1") // extra parameter
            );

            var result = _engine.ResolveTypeCode(data);

            Assert.Equal("X", result);
        }


    }
}
```

- Business\RenameEngineTests.cs

```cs
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
```


## 📁 Directory: ExcelInterop

- ExcelInterop\ExcelReaderTests.cs

```cs
using System;
using System.Collections.Generic;
using Xunit;
using JNOT.FileRenamer.ExcelInterop;

namespace JNOT.FileRenamer.Tests.ExcelInterop
{
    public class ExcelReaderTests
    {
        //
        // Helper: builds a grid with given dimensions
        //
        private string[,] Grid(int rows, int cols)
            => new string[rows, cols];

        //
        // Test 1: Sample date extraction (passed through from ExcelReader)
        //
        [Fact]
        public void Parser_ShouldExtractSampleDate()
        {
            var grid = Grid(20, 10);

            // Sample date is passed directly to Parse()
            var parser = new PivotParser();
            var pivot = parser.Parse(grid, "2025-11-20", "762-1234-1");

            Assert.Equal("2025-11-20", pivot.SampleDateRaw);
        }

        //
        // Test 2: Detects header row ("Parameter")
        //
        [Fact]
        public void Parser_ShouldFindParameterHeader()
        {
            var grid = Grid(20, 10);

            // Real Eurofins header row
            grid[10, 0] = "Parameter";
            grid[10, 1] = "Reporting Units";

            var parser = new PivotParser();
            var pivot = parser.Parse(grid, "2025-11-20", "762-1234-1");

            Assert.NotNull(pivot.Pairs);
        }

        //
        // Test 3: Extracts site IDs from Sample ID row
        //
        [Fact]
        public void Parser_ShouldExtractSiteIds()
        {
            var grid = Grid(20, 10);

            // Sample ID row (real Eurofins structure)
            grid[3, 0] = "Sample ID";
            grid[3, 2] = "INF-1";
            grid[3, 3] = "EFA-1";

            // Header row
            grid[5, 0] = "Parameter";
            grid[5, 1] = "Reporting Units";

            // Parameter row
            grid[7, 0] = "Total Suspended Solids";
            grid[7, 1] = "mg/l";
            grid[7, 2] = "140";
            grid[7, 3] = "1.3";

            // Termination
            grid[9, 0] = "Notes:";

            var parser = new PivotParser();
            var pivot = parser.Parse(grid, "2025-11-20", "762-1234-1");

            Assert.Contains(pivot.Pairs, p => p.SiteId == "INF-1");
            Assert.Contains(pivot.Pairs, p => p.SiteId == "EFA-1");
        }

        //
        // Test 4: Skips group headers like "Wet Chemistry by ..."
        //
        [Fact]
        public void Parser_ShouldSkipGroupHeaders()
        {
            var grid = Grid(30, 10);

            // Sample ID row
            grid[3, 0] = "Sample ID";
            grid[3, 2] = "INF-1";

            // Header row
            grid[5, 0] = "Parameter";
            grid[5, 1] = "Reporting Units";

            // Group header
            grid[7, 0] = "Wet Chemistry by 2540D-2020";

            // Parameter row
            grid[8, 0] = "Total Suspended Solids";
            grid[8, 1] = "mg/l";
            grid[8, 2] = "140";

            // Termination
            grid[10, 0] = "Notes:";

            var parser = new PivotParser();
            var pivot = parser.Parse(grid, "2025-11-20", "762-1234-1");

            Assert.Contains(pivot.Pairs, p => p.Parameter == "Total Suspended Solids");
            Assert.DoesNotContain(pivot.Pairs, p => p.Parameter.Contains("Wet Chemistry"));
        }

        //
        // Test 5: Creates correct parameter/site pairs
        //
        [Fact]
        public void Parser_ShouldCreatePairsForEachSiteWithValue()
        {
            var grid = Grid(30, 10);

            // Sample ID row
            grid[3, 0] = "Sample ID";
            grid[3, 2] = "INF-1";
            grid[3, 3] = "EFA-1";

            // Header row
            grid[5, 0] = "Parameter";
            grid[5, 1] = "Reporting Units";

            // Parameter row
            grid[7, 0] = "TSS";
            grid[7, 1] = "mg/l";
            grid[7, 2] = "140";
            grid[7, 3] = "1.3";

            // Termination
            grid[9, 0] = "Notes:";

            var parser = new PivotParser();
            var pivot = parser.Parse(grid, "2025-11-20", "762-1234-1");

            Assert.Contains(pivot.Pairs, p => p.Parameter == "TSS" && p.SiteId == "INF-1");
            Assert.Contains(pivot.Pairs, p => p.Parameter == "TSS" && p.SiteId == "EFA-1");
        }

        //
        // Test 6: Blank rows are allowed and skipped
        //
        [Fact]
        public void Parser_ShouldSkipBlankRowsBetweenParameterBlocks()
        {
            var grid = Grid(40, 10);

            // Sample ID row
            grid[3, 0] = "Sample ID";
            grid[3, 2] = "INF-1";

            // Header row
            grid[5, 0] = "Parameter";
            grid[5, 1] = "Reporting Units";

            // Parameter 1
            grid[7, 0] = "TSS";
            grid[7, 1] = "mg/l";
            grid[7, 2] = "140";

            // Blank row
            grid[8, 0] = "";

            // Parameter 2
            grid[9, 0] = "CBOD";
            grid[9, 1] = "mg/l";
            grid[9, 2] = "190";

            // Termination
            grid[12, 0] = "Notes:";

            var parser = new PivotParser();
            var pivot = parser.Parse(grid, "2025-11-20", "762-1234-1");

            Assert.Contains(pivot.Pairs, p => p.Parameter == "TSS");
            Assert.Contains(pivot.Pairs, p => p.Parameter == "CBOD");
        }

        //
        // Test 7: Stops at Notes:
        //
        [Fact]
        public void Parser_ShouldStopAtNotesSection()
        {
            var grid = Grid(40, 10);

            // Sample ID row
            grid[3, 0] = "Sample ID";
            grid[3, 2] = "INF-1";

            // Header row
            grid[5, 0] = "Parameter";
            grid[5, 1] = "Reporting Units";

            // Parameter row
            grid[7, 0] = "TSS";
            grid[7, 1] = "mg/l";
            grid[7, 2] = "140";

            // Termination
            grid[12, 0] = "Notes:";

            // Should NOT be read
            grid[14, 0] = "CBOD";

            var parser = new PivotParser();
            var pivot = parser.Parse(grid, "2025-11-20", "762-1234-1");

            Assert.Single(pivot.Pairs);
        }
    }
}
```

- FileRenamer.Tests.csproj
- TempFolder.cs

```cs
using System;
using System.IO;

public sealed class TempFolder : IDisposable
{
    public string Path { get; }

    public TempFolder()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "FRPP_" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
        catch
        {
            // swallow cleanup errors
        }
    }
}
```

- UnitTest1.cs

```cs
namespace JNOT.FileRenamer.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }
    }
}

```

- xunit.runner.json
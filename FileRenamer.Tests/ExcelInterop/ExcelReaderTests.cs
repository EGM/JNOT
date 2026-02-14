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

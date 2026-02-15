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

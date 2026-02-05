
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
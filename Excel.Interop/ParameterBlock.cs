using System;
using System.Collections.Generic;

namespace Jnot.Excel.Interop
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

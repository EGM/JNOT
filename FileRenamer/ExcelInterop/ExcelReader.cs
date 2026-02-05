using System;
using Microsoft.Office.Interop.Excel;

namespace JNOT.FileRenamer.ExcelInterop
{
    public class ExcelReader
    {
        private readonly PivotParser _parser = new PivotParser();

        public PivotData ReadPivot(string filePath)
        {
            Application app = null;
            Workbook wb = null;

            try
            {
                app = new Application { Visible = false, DisplayAlerts = false };
                wb = app.Workbooks.Open(filePath);

                Worksheet ws = wb.Sheets[1];
                Range used = ws.UsedRange;

                int rows = used.Rows.Count;
                int cols = used.Columns.Count;

                // Build a string[,] grid for the parser
                var grid = new string[rows, cols];

                for (int r = 1; r <= rows; r++)
                {
                    for (int c = 1; c <= cols; c++)
                    {
                        object raw = used.Cells[r, c].Value2;
                        grid[r - 1, c - 1] = raw?.ToString() ?? "";
                    }
                }

                // Sample date is always at C4 (row 4, col 3 in Excel → [3,2] in zero-based)
                string sampleDate = grid[3, 2];

                // Job number comes from filename
                string jobNumber = ExtractJobNumberFromFilename(filePath);

                // Parse pivot using pure logic
                return _parser.Parse(grid, sampleDate, jobNumber);
            }
            finally
            {
                wb?.Close(false);
                app?.Quit();
            }
        }

        private string ExtractJobNumberFromFilename(string filePath)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(filePath);
            var parts = name.Split('_');
            return parts.Length > 0 ? parts[0] : string.Empty;
        }
    }
}
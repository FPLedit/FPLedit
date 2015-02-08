using Buchfahrplan.FileModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Drawing;

namespace Buchfahrplan.Export
{
    public static class ExcelExport
    {
        public static void Export(Timetable timetable, string filename, ExportFileType filetype)
        {            
            Excel.Application objExcel = new Excel.Application();
            Excel.Workbook workbook = objExcel.Workbooks.Add();
            Excel.Worksheet worksheet = workbook.Worksheets["Tabelle1"];
            objExcel.Visible = true;           

            int line = 1;
            foreach (Train train in timetable.Trains)
            {
                SetColumnWidth(worksheet, 3, 40);

                // Kopf zeichnen
                for (int i = 0; i <= 4; i++)
                {
                    switch (i)
                    {
                        case 0:
                            MergeCells(worksheet, GetRangeName(line, 1), GetRangeName(line, 5));
                            worksheet.Cells[line, 1] = train.Name;
                            SetTextAlignment(worksheet.Cells[line, 1], HTextAlignment.Center, VTextAlignment.Middle);
                            SetFont(worksheet.Cells[line, 1], new Font("DIN 1451 Mittelschrift Alt", 15));
                            break;

                        case 1:
                            MergeCells(worksheet, GetRangeName(line, 1), GetRangeName(line, 5));
                            worksheet.Cells[line, 1] = train.Line;
                            SetTextAlignment(worksheet.Cells[line, 1], HTextAlignment.Center, VTextAlignment.Middle);
                            SetFont(worksheet.Cells[line, 1], new Font("DIN 1451 Mittelschrift Alt", 12));
                            break;

                        case 2:
                            worksheet.Cells[line, 1] = "Tfz " + train.Locomotive;
                            SetFont(worksheet.Cells[line, 1], new Font("DIN 1451 Mittelschrift Alt", 11));
                            break;

                        case 3:
                            for (int x = 1; x <= 5; x++)
                            {
                                worksheet.Cells[line, x] = x;
                                SetTextAlignment(worksheet.Cells[line, x], HTextAlignment.Center, VTextAlignment.Top);
                                SetCellBorder(worksheet.Cells[line, x], BorderType.Thick, BorderType.Thick, BorderType.Thick,
                                    BorderType.Normal);
                                SetFont(worksheet.Cells[line, x], new Font("DIN 1451 Mittelschrift Alt", 11));
                            }
                            break;

                        case 4:
                            SetRowHeight(worksheet, line, 100);

                            worksheet.Cells[line, 1] = "Lage\nder\nBetriebs-\nstelle\n\n(km)";
                            SetTextAlignment(worksheet.Cells[line, 1], HTextAlignment.Center, VTextAlignment.Top);
                            SetCellBorder(worksheet.Cells[line, 1], BorderType.Thick, BorderType.Normal, BorderType.Thick, BorderType.Thick);
                            SetFont(worksheet.Cells[line, 1], new Font("DIN 1451 Mittelschrift Alt", 11));

                            worksheet.Cells[line, 2] = "Höchst-\nGeschwin-\ndigkeit\n\n\n(km/h)";
                            SetTextAlignment(worksheet.Cells[line, 2], HTextAlignment.Center, VTextAlignment.Top);
                            SetCellBorder(worksheet.Cells[line, 2], BorderType.Thick, BorderType.Normal, BorderType.Thick, BorderType.Thick);
                            SetFont(worksheet.Cells[line, 2], new Font("DIN 1451 Mittelschrift Alt", 11));

                            worksheet.Cells[line, 3] = "Betriebsstellen,\nständige Langsamfahrstellen,\nverkürzter Vorsignalabstand";
                            SetTextAlignment(worksheet.Cells[line, 3], HTextAlignment.Left, VTextAlignment.Top);
                            SetCellBorder(worksheet.Cells[line, 3], BorderType.Thick, BorderType.Normal, BorderType.Thick, BorderType.Thick);
                            SetFont(worksheet.Cells[line, 3], new Font("DIN 1451 Mittelschrift Alt", 11));

                            worksheet.Cells[line, 4] = "Ankunft";
                            SetTextAlignment(worksheet.Cells[line, 4], HTextAlignment.Center, VTextAlignment.Top);
                            SetCellBorder(worksheet.Cells[line, 4], BorderType.Thick, BorderType.Normal, BorderType.Thick, BorderType.Thick);
                            SetFont(worksheet.Cells[line, 4], new Font("DIN 1451 Mittelschrift Alt", 11));

                            worksheet.Cells[line, 5] = "Abfahrt\noder Durch-\nfahrt";
                            SetTextAlignment(worksheet.Cells[line, 5], HTextAlignment.Left, VTextAlignment.Top);
                            SetCellBorder(worksheet.Cells[line, 5], BorderType.Thick, BorderType.Normal, BorderType.Thick, BorderType.Thick);
                            SetFont(worksheet.Cells[line, 5], new Font("DIN 1451 Mittelschrift Alt", 11));
                            break;
                    }
                    line++;
                }

                if (!train.Negative)
                {
                    // Stationen und An- Abfahrten Zeichnen
                    foreach (var station in timetable.Stations.OrderBy(s => s.Kilometre))
                    {
                        worksheet.Cells[line, 1] = station.Kilometre.ToString("0.0");
                        SetFont(worksheet.Cells[line, 1], new Font("DIN 1451 Mittelschrift Alt", 11));
                        SetTextAlignment(worksheet.Cells[line, 1], HTextAlignment.Center, VTextAlignment.Middle);
                        SetCellBorder(worksheet.Cells[line, 1], BorderType.Thick, BorderType.CurrentStatus, BorderType.Thick,
                            BorderType.CurrentStatus);

                        worksheet.Cells[line, 2] = station.MaxVelocity.ToString("#");
                        SetFont(worksheet.Cells[line, 2], new Font("DIN 1451 Mittelschrift Alt", 11));
                        SetTextAlignment(worksheet.Cells[line, 2], HTextAlignment.Center, VTextAlignment.Middle);
                        SetCellBorder(worksheet.Cells[line, 2], BorderType.Thick, BorderType.Thick, BorderType.Thick,
                            BorderType.CurrentStatus);

                        worksheet.Cells[line, 3] = station.Name;
                        SetFont(worksheet.Cells[line, 3], new Font("DIN 1451 Mittelschrift Alt", 11));
                        SetCellBorder(worksheet.Cells[line, 3], BorderType.Thick, BorderType.CurrentStatus, BorderType.Thick,
                            BorderType.CurrentStatus);

                        try { worksheet.Cells[line, 4] = train.Arrivals[station].ToShortTimeString(); }
                        catch { }
                        SetFont(worksheet.Cells[line, 4], new Font("DIN 1451 Mittelschrift Alt", 11));
                        SetTextAlignment(worksheet.Cells[line, 4], HTextAlignment.Center, VTextAlignment.Middle);
                        SetCellBorder(worksheet.Cells[line, 4], BorderType.Thick, BorderType.CurrentStatus, BorderType.Thick,
                            BorderType.CurrentStatus);

                        try { worksheet.Cells[line, 5] = train.Departures[station].ToShortTimeString(); }
                        catch { }
                        SetFont(worksheet.Cells[line, 5], new Font("DIN 1451 Mittelschrift Alt", 11));
                        SetTextAlignment(worksheet.Cells[line, 5], HTextAlignment.Center, VTextAlignment.Middle);
                        SetCellBorder(worksheet.Cells[line, 5], BorderType.Thick, BorderType.CurrentStatus, BorderType.Thick,
                            BorderType.CurrentStatus);

                        line++;
                    }
                }

                if (train.Negative)
                {
                    // Stationen und An- Abfahrten Zeichnen
                    foreach (var station in timetable.Stations.OrderBy(s => s.Kilometre).Reverse())
                    {
                        worksheet.Cells[line, 1] = station.Kilometre.ToString("0.0");
                        SetFont(worksheet.Cells[line, 1], new Font("DIN 1451 Mittelschrift Alt", 11));
                        SetTextAlignment(worksheet.Cells[line, 1], HTextAlignment.Center, VTextAlignment.Middle);
                        SetCellBorder(worksheet.Cells[line, 1], BorderType.Thick, BorderType.CurrentStatus, BorderType.Thick,
                            BorderType.CurrentStatus);

                        worksheet.Cells[line, 2] = station.MaxVelocity.ToString("#");
                        SetFont(worksheet.Cells[line, 2], new Font("DIN 1451 Mittelschrift Alt", 11));
                        SetTextAlignment(worksheet.Cells[line, 2], HTextAlignment.Center, VTextAlignment.Middle);
                        SetCellBorder(worksheet.Cells[line, 2], BorderType.Thick, BorderType.Thick, BorderType.Thick,
                            BorderType.CurrentStatus);

                        worksheet.Cells[line, 3] = station.Name;
                        SetFont(worksheet.Cells[line, 3], new Font("DIN 1451 Mittelschrift Alt", 11));
                        SetCellBorder(worksheet.Cells[line, 3], BorderType.Thick, BorderType.CurrentStatus, BorderType.Thick,
                            BorderType.CurrentStatus);

                        try { worksheet.Cells[line, 4] = train.Arrivals[station].ToShortTimeString(); }
                        catch { }
                        SetFont(worksheet.Cells[line, 4], new Font("DIN 1451 Mittelschrift Alt", 11));
                        SetTextAlignment(worksheet.Cells[line, 4], HTextAlignment.Center, VTextAlignment.Middle);
                        SetCellBorder(worksheet.Cells[line, 4], BorderType.Thick, BorderType.CurrentStatus, BorderType.Thick,
                            BorderType.CurrentStatus);

                        try { worksheet.Cells[line, 5] = train.Departures[station].ToShortTimeString(); }
                        catch { }
                        SetFont(worksheet.Cells[line, 5], new Font("DIN 1451 Mittelschrift Alt", 11));
                        SetTextAlignment(worksheet.Cells[line, 5], HTextAlignment.Center, VTextAlignment.Middle);
                        SetCellBorder(worksheet.Cells[line, 5], BorderType.Thick, BorderType.CurrentStatus, BorderType.Thick,
                            BorderType.CurrentStatus);

                        line++;
                    }
                }

                for (int x = 1; x <= 5; x++)
                {
                    SetCellBorder(worksheet.Cells[line, x], BorderType.Thick, BorderType.CurrentStatus, BorderType.Thick,
                        BorderType.Thick);
                }

                // Leerzeilen nach einem Fahrplan
                line += 3;

                worksheet.HPageBreaks.Add(worksheet.Range["A"+ line.ToString()]);

            }
            
            switch (filetype)
            {
                case ExportFileType.XlFile:
                    workbook.SaveAs(filename);
                    objExcel.Visible = true;
                    break;
                case ExportFileType.PdfFile:
                    workbook.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, filename,
                        Excel.XlFixedFormatQuality.xlQualityStandard, true, true, Type.Missing, Type.Missing, true, Type.Missing);
                    break;
            }

            workbook.Close();
            objExcel.Quit();

            worksheet = null;
            workbook = null;
            objExcel = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


        private static void SetCellBorder(Excel.Range range, BorderType leftBorder, BorderType topBorder, BorderType rightBorder,
                BorderType bottomBorder)
        {
            Excel.Borders border = range.Borders;

            switch (leftBorder)
            {
                case BorderType.None:
                    border[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlLineStyleNone;
                    break;
                case BorderType.Normal:
                    border[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border[Excel.XlBordersIndex.xlEdgeLeft].Weight = Excel.XlBorderWeight.xlThin;
                    break;
                case BorderType.Thick:
                    border[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border[Excel.XlBordersIndex.xlEdgeLeft].Weight = Excel.XlBorderWeight.xlThick;
                    break;
            }

            switch (topBorder)
            {
                case BorderType.None:
                    border[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlLineStyleNone;
                    break;
                case BorderType.Normal:
                    border[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border[Excel.XlBordersIndex.xlEdgeTop].Weight = Excel.XlBorderWeight.xlThin;
                    break;
                case BorderType.Thick:
                    border[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border[Excel.XlBordersIndex.xlEdgeTop].Weight = Excel.XlBorderWeight.xlThick;
                    break;
            }

            switch (bottomBorder)
            {
                case BorderType.None:
                    border[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlLineStyleNone;
                    break;
                case BorderType.Normal:
                    border[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border[Excel.XlBordersIndex.xlEdgeBottom].Weight = Excel.XlBorderWeight.xlThin;
                    break;
                case BorderType.Thick:
                    border[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border[Excel.XlBordersIndex.xlEdgeBottom].Weight = Excel.XlBorderWeight.xlThick;
                    break;
            }

            switch (rightBorder)
            {
                case BorderType.None:
                    border[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlLineStyleNone;
                    break;
                case BorderType.Normal:
                    border[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border[Excel.XlBordersIndex.xlEdgeRight].Weight = Excel.XlBorderWeight.xlThin;
                    break;
                case BorderType.Thick:
                    border[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border[Excel.XlBordersIndex.xlEdgeRight].Weight = Excel.XlBorderWeight.xlThick;
                    break;
            }
        }

        private static void SetColumnWidth(Excel.Worksheet worksheet, int column, int width)
        {
            ((Excel.Range)worksheet.Cells[1, column]).EntireColumn.ColumnWidth = width;
        }

        private static void SetFont(Excel.Range range, Font font)
        {
            range.Font.Name = font.Name;
            range.Font.Size = font.Size;

            range.Font.Bold = font.Bold;
            range.Font.Italic = font.Italic;
            range.Font.Underline = font.Underline;
        }

        private static void SetTextAlignment(Excel.Range range, HTextAlignment hAlignment, VTextAlignment vAlignment)
        {
            switch (hAlignment)
            {
                case HTextAlignment.Left:
                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                    break;
                case HTextAlignment.Center:
                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    break;
                case HTextAlignment.Right:
                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    break;
            }

            switch (vAlignment)
            {
                case VTextAlignment.Top:
                    range.VerticalAlignment = Excel.XlVAlign.xlVAlignTop;
                    break;
                case VTextAlignment.Middle:
                    range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    break;
                case VTextAlignment.Bottom:
                    range.VerticalAlignment = Excel.XlVAlign.xlVAlignBottom;
                    break;
            }
        }

        private static void SetRowHeight(Excel.Worksheet worksheet, int row, int height)
        {
            ((Excel.Range)worksheet.Cells[row, 1]).EntireRow.RowHeight = height;
        }

        private static void MergeCells(Excel.Worksheet worksheet, string beginCell, string endCell)
        {
            worksheet.get_Range(beginCell, endCell).Merge(Type.Missing);
        }

        private static string GetRangeName(int row, int column)
        {
            string[] columns = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", 
                "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            return columns[column - 1] + row.ToString();
        }
    }

    public enum BorderType
    {
        Normal,
        Thick,
        None,
        CurrentStatus
    }

    public enum HTextAlignment
    {
        Left,
        Center,
        Right
    }

    public enum VTextAlignment
    {
        Top,
        Middle,
        Bottom
    }

    public enum ExportFileType
    {
        XlFile,
        PdfFile
    }
}

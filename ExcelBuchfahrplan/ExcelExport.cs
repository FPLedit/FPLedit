using Buchfahrplan.FileModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Buchfahrplan.BuchfahrplanExport
{
    public class ExcelExport : IExport
    {
        private static Font headingFont = new Font("DIN 1451 Mittelschrift Alt", 15);
        private static Font subHeadingFont = new Font("DIN 1451 Mittelschrift Alt", 12);
        private static Font cellFont = new Font("DIN 1451 Mittelschrift Alt", 11);

        public void Export(Timetable timetable, string filename)
        {
            Excel.Application objExcel = new Excel.Application();
            Excel.Workbook workbook = objExcel.Workbooks.Add();
            Excel.Worksheet worksheet = workbook.Worksheets["Tabelle1"];
            Excel.HPageBreaks hBreaks = worksheet.HPageBreaks;
            objExcel.Visible = true;            

            int lineCount = timetable.Trains.Count * (timetable.Stations.Count + 8);
            int columnCount = 5;

            string[,] cellBuffer = new string[lineCount, columnCount];

            #region cellBuffer
            // Baue cellBuffer
            int cellBufferLine = 0;
            foreach (Train train in timetable.Trains)
            {
                // Kopf zeichnen
                cellBuffer[cellBufferLine, 0] = train.Name;
                cellBufferLine++;

                cellBuffer[cellBufferLine, 0] = train.Line;
                cellBufferLine++;

                cellBuffer[cellBufferLine, 0] = "Tfz " + train.Locomotive;
                cellBufferLine++;

                for (int x = 0; x <= 4; x++)
                {
                    cellBuffer[cellBufferLine, x] = x.ToString();
                }
                cellBufferLine++;

                cellBuffer[cellBufferLine, 0] = "Lage\nder\nBetriebs-\nstelle\n\n(km)";
                cellBuffer[cellBufferLine, 1] = "Höchst-\nGeschwin-\ndigkeit\n\n\n(km/h)";
                cellBuffer[cellBufferLine, 2] = "Betriebsstellen,\nständige Langsamfahrstellen,\nverkürzter Vorsignalabstand";
                cellBuffer[cellBufferLine, 3] = "Ankunft";
                cellBuffer[cellBufferLine, 4] = "Abfahrt\noder Durch-\nfahrt";
                cellBufferLine++;

                if (!train.Negative)
                {
                    // Stationen und An- Abfahrten Zeichnen
                    foreach (var station in timetable.Stations.OrderBy(s => s.Kilometre))
                    {
                        cellBuffer[cellBufferLine, 0] = station.Kilometre.ToString("0.0");

                        cellBuffer[cellBufferLine, 1] = station.MaxVelocity.ToString("#");

                        cellBuffer[cellBufferLine, 2] = station.Name;
       
                        try { cellBuffer[cellBufferLine, 3] = train.Arrivals[station].ToShortTimeString(); }
                        catch { }
   
                        try { cellBuffer[cellBufferLine, 4] = train.Departures[station].ToShortTimeString(); }
                        catch { }

                        cellBufferLine++;
                    }
                }

                if (train.Negative)
                {
                    // Stationen und An- Abfahrten Zeichnen
                    foreach (var station in timetable.Stations.OrderBy(s => s.Kilometre).Reverse())
                    {
                        cellBuffer[cellBufferLine, 0] = station.Kilometre.ToString("0.0");

                        cellBuffer[cellBufferLine, 1] = station.MaxVelocity.ToString("#");

                        cellBuffer[cellBufferLine, 2] = station.Name;

                        try { cellBuffer[cellBufferLine, 3] = train.Arrivals[station].ToShortTimeString(); }
                        catch { }

                        try { cellBuffer[cellBufferLine, 4] = train.Departures[station].ToShortTimeString(); }
                        catch { }

                        cellBufferLine++;
                    }
                }

                // Leerzeilen nach einem Fahrplan
                cellBufferLine += 3;
            }

            Excel.Range range = worksheet.Range["A1:E" + lineCount.ToString()];
            range.set_Value(Excel.XlRangeValueDataType.xlRangeValueDefault, cellBuffer);
            Marshal.ReleaseComObject(range);

            #endregion

            // Formatieren
            // 3. Spalte breit
            Excel.Range cell = worksheet.Cells[1, 3];
            Excel.Range col = cell.EntireColumn;
            col.ColumnWidth = 40;

            Marshal.ReleaseComObject(col);
            Marshal.ReleaseComObject(cell);

            int line = 1;
            foreach (Train train in timetable.Trains)
            {
                SetTextAlignment(worksheet.Range["A" + line.ToString() + ":A" + (line + 1).ToString()], 
                    HTextAlignment.Center, VTextAlignment.Middle);

                // Kopf zeichnen
                // Zugnummer
                worksheet.Range["A" + line.ToString() + ":E" + line.ToString()].Merge(Type.Missing);
                SetFont(worksheet.Cells[line, 1], headingFont);
                line++;

                // Strecke
                worksheet.Range["A" + line.ToString() + ":E" + line.ToString()].Merge(Type.Missing);
                SetFont(worksheet.Cells[line, 1], subHeadingFont);
                line++;

                // Tfz
                SetFont(worksheet.Cells[line, 1], cellFont);
                line++;

                // Spaltennummern
                Excel.Range range1 = worksheet.Range["A" + line.ToString() + ":E" + (line + 1).ToString()];
                SetTextAlignment(range1,  HTextAlignment.Center, VTextAlignment.Top);
                SetFont(range1, cellFont);

                Excel.Borders border1 = range1.Borders;

                border1[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                border1[Excel.XlBordersIndex.xlEdgeLeft].Weight = Excel.XlBorderWeight.xlThick;

                border1[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                border1[Excel.XlBordersIndex.xlEdgeTop].Weight = Excel.XlBorderWeight.xlThick;

                border1[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                border1[Excel.XlBordersIndex.xlEdgeRight].Weight = Excel.XlBorderWeight.xlThick;

                border1[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                border1[Excel.XlBordersIndex.xlEdgeBottom].Weight = Excel.XlBorderWeight.xlThick;

                border1[Excel.XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;
                border1[Excel.XlBordersIndex.xlInsideVertical].Weight = Excel.XlBorderWeight.xlThick;

                border1[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = Excel.XlLineStyle.xlContinuous;
                border1[Excel.XlBordersIndex.xlInsideHorizontal].Weight = Excel.XlBorderWeight.xlThin;

                Marshal.ReleaseComObject(border1);
                Marshal.ReleaseComObject(range1);
                line++;

                // Spaltenerklärungen
                Excel.Range cell2 = worksheet.Cells[line, 1];
                Excel.Range row = cell2.EntireRow;
                row.RowHeight = 100;

                Marshal.ReleaseComObject(row);
                Marshal.ReleaseComObject(cell2);
                line++;

                // Stationen und An- Abfahrten Zeichnen
                foreach (var station in timetable.Stations.OrderBy(s => s.Kilometre))
                {
                    Excel.Range range3 = worksheet.Range["A" + line.ToString() + ":E" + line.ToString()];
                    SetFont(range3, cellFont);
                    SetTextAlignment(range3, HTextAlignment.Center, VTextAlignment.Top);

                    Excel.Borders border3 = range3.Borders;

                    border3[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border3[Excel.XlBordersIndex.xlEdgeLeft].Weight = Excel.XlBorderWeight.xlThick;

                    border3[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border3[Excel.XlBordersIndex.xlEdgeRight].Weight = Excel.XlBorderWeight.xlThick;

                    border3[Excel.XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;
                    border3[Excel.XlBordersIndex.xlInsideVertical].Weight = Excel.XlBorderWeight.xlThick;

                    Marshal.ReleaseComObject(border3);
                    Marshal.ReleaseComObject(range3);

                    if (station.MaxVelocity != 0)
                    {
                        Excel.Range range4 = worksheet.Cells[line, 2];
                        Excel.Borders border4 = range4.Borders;

                        border4[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                        border4[Excel.XlBordersIndex.xlEdgeTop].Weight = Excel.XlBorderWeight.xlThick;

                        Marshal.ReleaseComObject(border4);
                        Marshal.ReleaseComObject(range4);
                    }

                    line++;
                }                

                // Ende der Tabelle
                Excel.Range range5 = worksheet.Range["A" + line.ToString() + ":E" + line.ToString()];
                SetTextAlignment(range5, HTextAlignment.Center, VTextAlignment.Top);
                SetFont(range5, cellFont);

                Excel.Borders border5 = range5.Borders;

                border5[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                border5[Excel.XlBordersIndex.xlEdgeLeft].Weight = Excel.XlBorderWeight.xlThick;

                border5[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                border5[Excel.XlBordersIndex.xlEdgeRight].Weight = Excel.XlBorderWeight.xlThick;

                border5[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                border5[Excel.XlBordersIndex.xlEdgeBottom].Weight = Excel.XlBorderWeight.xlThick;

                border5[Excel.XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;
                border5[Excel.XlBordersIndex.xlInsideVertical].Weight = Excel.XlBorderWeight.xlThick;

                Marshal.ReleaseComObject(border5);
                Marshal.ReleaseComObject(range5);

                // Leerzeilen nach einem Fahrplan
                line += 3;

                hBreaks.Add(worksheet.Range["A" + line.ToString()]);
            }

            ExportFileType filetype = ExportFileType.XlFile;

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

            // Alle Objekte freigeben
            Marshal.ReleaseComObject(hBreaks);
            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(objExcel);

            worksheet = null;
            workbook = null;
            objExcel = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static void SetFont(Excel.Range range, Font font)
        {
            range.Font.Name = font.Name;
            range.Font.Size = font.Size;
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

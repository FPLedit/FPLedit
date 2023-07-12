using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using System;
using System.IO;
using FPLedit.Shared.Rendering;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PageOrientation = PdfSharp.PageOrientation;

namespace FPLedit.Bildfahrplan.Forms;

internal sealed class PrintRenderer
{
    private readonly IPluginInterface pluginInterface;
    private readonly Timetable tt;
    private readonly TimetableStyle attrs;

    public PrintRenderer(IPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        this.tt = pluginInterface.Timetable;
        attrs = new TimetableStyle(tt);
    }

    public void InitPrint()
    {
        var form = new PrintForm(pluginInterface);
        var printSettings = form.ShowModal();
        if (printSettings == null)
            return;

        if (pluginInterface.Timetable != tt)
            throw new InvalidOperationException("timetable changed while printing!");
        var pd = VirtualRoute.GetPathDataMaybeVirtual(tt, printSettings.Value.routeIdx);

        var size = printSettings.Value.pageSize;
        var orientation = printSettings.Value.landscape ? PageOrientation.Landscape : PageOrientation.Portrait;
        var marginCm = printSettings.Value.marginCm;

        using var exportFileDialog = new SaveFileDialog { Title = T._("Bildfahrplan als PDF drucken") };
        exportFileDialog.Filters.Add(new FileFilter(T._("PDF-Datei"), "*.pdf"));
        if (pluginInterface.FileState.FileName != null)
        {
            exportFileDialog.Directory = new Uri(Path.GetDirectoryName(pluginInterface.FileState.FileName)!);
            exportFileDialog.FileName = Path.ChangeExtension(pluginInterface.FileState.FileName, "pdf");
        }

        if (exportFileDialog.ShowDialog((Window) pluginInterface.RootForm) != DialogResult.Ok)
            return;

        var exportFilename = exportFileDialog.FileName;
        if (exportFilename == null)
            return;

        PrintDocument(exportFilename, size, orientation, marginCm, pd);
    }

    private void PrintDocument(string exportFilename, PageSize size, PageOrientation orientation, float marginCm, Func<PathData> pd)
    {
        pluginInterface.Logger.Info(T._("Drucke als PDF {0}...", exportFilename));

        try
        {
            using var doc = new PdfDocument();
            doc.Info.Title = T._("Bildfahrplan generiert mit FPLedit");
            doc.Info.Creator = "FPLedit";

            var needsMorePages = true;
            TimeEntry? startTime = null;
            while (needsMorePages)
            {
                (needsMorePages, var nextStartTime) = PrintPage(doc, size, orientation, marginCm, startTime, pd);
                if (needsMorePages && nextStartTime == startTime)
                    throw new Exception(T._("Die Druckeinstellungen erzeugen unendlich viele Seiten!"));
                startTime = nextStartTime;
            }

            doc.Save(exportFilename);
            pluginInterface.Logger.Info(T._("Drucken abgeschlossen..."));
        }
        catch (Exception ex)
        {
            pluginInterface.Logger.Error(T._("Fehler beim Drucken! {0}", ex.Message));
        }
    }

    private (bool, TimeEntry) PrintPage(PdfDocument doc, PageSize size, PageOrientation orientation, float marginCm, TimeEntry? startTime, Func<PathData> pd)
    {
        var page = doc.AddPage();
        page.Size = size;
        page.Orientation = orientation;

        var renderer = new Renderer(tt, pd);
        double height = page.Height;
        double width = page.Width;

        var marginLength = new XUnit(marginCm, XGraphicsUnit.Centimeter);
        var margin = new Margins((float) marginLength, (float) marginLength, (float) marginLength, (float) marginLength);
        renderer.SetMargins(margin);

        var start = startTime ?? attrs.StartTime;
        using var g = XGraphics.FromPdfPage(page);
        var g2 = new MGraphicsPdfSharp(g);
        var curEndTime = GetTimeByHeight(g2, renderer, start, (int)height);

        renderer.Draw(g2, start, curEndTime, true, (int)width);

        var endTime = GetEndTime(start, attrs.EndTime);
        if (curEndTime < endTime)
            return (true, curEndTime);
        return (false, curEndTime);
    }

    private TimeEntry GetTimeByHeight(IMGraphics g, Renderer renderer, TimeEntry start, int height)
    {
        var endTime = GetEndTime(start, attrs.EndTime);
        var fillUpMinutes = (byte) (60 - start.Minutes);
        var restMinutes = endTime.Minutes;
            
        var headerHeight = renderer.GetHeight(g, default, default, true);

        var spanMinutes = endTime.GetTotalMinutes() - start.GetTotalMinutes();

        TimeEntry ClipEnd(TimeEntry calcedEnd)
        {
            if (calcedEnd >= endTime) return endTime;
            if (calcedEnd < start) return start;
            return calcedEnd;
        }

        if (fillUpMinutes * (attrs.HeightPerHour / 60f) + headerHeight > height) // Not even the next hour does fit on the page.
        {
            var end =  start + new TimeEntry(0, (int) ((height - headerHeight) / (attrs.HeightPerHour / 60f)));
            return ClipEnd(end);
        }

        var fullTimeHeight = height - headerHeight;

        if (fullTimeHeight < attrs.HeightPerHour) // Not even 1 full hour does fit on the page.
        {
            int minutes = (int) (fullTimeHeight / (attrs.HeightPerHour / 60f));
            var end = start + new TimeEntry(0, minutes);
            return ClipEnd(end);
        }

        var fullHourHeight = fullTimeHeight - fillUpMinutes * (attrs.HeightPerHour / 60f);
        int fullHours = (int) (fullHourHeight / (attrs.HeightPerHour));

        if (fullHourHeight - fullHours * attrs.HeightPerHour > (attrs.HeightPerHour / 60f) * restMinutes && fullHours * 60 + fillUpMinutes + restMinutes >= spanMinutes)
        {
            var end = start + new TimeEntry(fullHours, restMinutes + fillUpMinutes);
            return ClipEnd(end);
        }

        return ClipEnd(start + new TimeEntry(fullHours, fillUpMinutes));
    }
        
    private TimeEntry GetEndTime(TimeEntry startTime, TimeEntry endTime)
        => endTime < startTime ? endTime + new TimeEntry(24, 0) : endTime;
}
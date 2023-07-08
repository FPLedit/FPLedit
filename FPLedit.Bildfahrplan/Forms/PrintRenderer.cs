using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PageOrientation = PdfSharp.PageOrientation;

namespace FPLedit.Bildfahrplan.Forms
{
    internal sealed class PrintRenderer
    {
        private readonly IPluginInterface pluginInterface;
        private readonly Timetable tt;
        private readonly TimetableStyle attrs;

        private TimeEntry? last;
        private Func<PathData> pd;

        public PrintRenderer(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            this.tt = pluginInterface.Timetable;
            attrs = new TimetableStyle(tt);
        }

        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void InitPrint()
        {
            using var doc = new PdfDocument();
            var form = new FDialog<DialogResult>();
            var routesDropDown = new RoutesDropDown();
            var routeStack = new StackLayout(routesDropDown) { Orientation = Orientation.Horizontal, Padding = new Eto.Drawing.Padding(10), Spacing = 5 };
            var routeGroup = new GroupBox { Content = routeStack, Text = T._("Strecke auswählen") };
            var paperDropDown = new DropDown();
            var landscapeChk = new CheckBox { Text = T._("Querformat") };
            var printButton = new Button { Text = T._("Drucken") };
            var printerStack = new StackLayout(paperDropDown, landscapeChk, printButton) { Orientation = Orientation.Horizontal, Padding = new Eto.Drawing.Padding(10), Spacing = 5 };
            var printerGroup = new GroupBox { Content = printerStack, Text = T._("Druckeinstellungen") };
            var stack = new StackLayout(routeGroup, printerGroup) { Orientation = Orientation.Vertical, Padding = new Eto.Drawing.Padding(10), Spacing = 5 };
            
            routesDropDown.Initialize(pluginInterface);
            routesDropDown.EnableVirtualRoutes = true;
            
            form.Content = stack;
            form.DefaultButton = printButton;
            form.Title = T._("Bildfahrplan drucken");

            printButton.Click += (_, _) =>
            {
                form.Result = DialogResult.Ok;
                form.Close();
            };

            var paperSizes = Enum.GetValues<PageSize>()
                .ToDictionary(e => e, e => Enum.GetName(e)!);

            var a4Index = Array.IndexOf(paperSizes.Values.ToArray(), "A4");
            paperDropDown.DataStore = paperSizes.Values;
            paperDropDown.SelectedIndex = (a4Index != -1) ? a4Index : 0;

            if (form.ShowModal() == DialogResult.Ok)
            {
                if (routesDropDown.SelectedRoute > Timetable.UNASSIGNED_ROUTE_ID)
                    pd = Renderer.DefaultPathData(routesDropDown.SelectedRoute, pluginInterface.Timetable);
                else
                {
                    var virt = VirtualRoute.GetVRoute(pluginInterface.Timetable, routesDropDown.SelectedRoute);
                    pd = virt!.GetPathData;
                }

                doc.Info.Title = T._("Bildfahrplan generiert mit FPLedit");

                var size = paperSizes.Single(p => p.Value == (string) paperDropDown.SelectedValue).Key;
                var orientation = landscapeChk.Checked!.Value ? PageOrientation.Landscape : PageOrientation.Portrait;

                try
                {
                    var needsMore = true;
                    var pgC = 0;  // safety measure.
                    while (needsMore && pgC < 10)
                    {
                        needsMore = PrintPage(doc, size, orientation);
                        pgC++;
                    }

                    doc.Save("./testout.pdf");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(T._("Fehler beim Drucken! {0}", ex.Message), "FPLedit", MessageBoxType.Error);
                }
            }
        }

        private bool PrintPage(PdfDocument doc, PageSize size, PageOrientation orientation)
        {
            var e = doc.AddPage();
            e.Size = size;
            e.TrimMargins = new TrimMargins {All = 20};
            e.Orientation = orientation;

            var renderer = new Renderer(tt, pd);
            double height = e.Height;
            double width = e.Width;

            var margin = new Margins(0f, 0f, 0f, 0f);
            renderer.SetMargins(margin);

            var start = last ?? attrs.StartTime;
            using var g = XGraphics.FromPdfPage(e);
            var g2 = new MGraphicsPdfSharp(g);
            last = GetTimeByHeight(g2, renderer, start, (int)height);

            renderer.Draw(g2, start, last.Value, true, (int)width);

            var endTime = GetEndTime(start, attrs.EndTime);
            if (last!.Value < endTime)
                return true;
            return false;
        }

        private TimeEntry GetTimeByHeight(IMGraphics g, Renderer renderer, TimeEntry start, int height)
        {
            var endTime = GetEndTime(start, attrs.EndTime);
            var fillUpMinutes = (byte) (60 - start.Minutes);
            var restMinutes = endTime.Minutes;
            
            var headerHeight = renderer.GetHeight(g, default, default, true);

            var spanMinutes = endTime.GetTotalMinutes() - start.GetTotalMinutes();

            if (fillUpMinutes * (attrs.HeightPerHour / 60f) + headerHeight > height) // Not even the next hour does fit on the page.
            {
                var end =  start + new TimeEntry(0, (int) ((height - headerHeight) / (attrs.HeightPerHour / 60f)));
                if (end >= endTime)
                    return endTime;
                return end;
            }

            var fullTimeHeight = height - headerHeight;

            if (fullTimeHeight < attrs.HeightPerHour) // Not even 1 full hour does fit on the page.
            {
                int minutes = (int) (fullTimeHeight / (attrs.HeightPerHour / 60f));
                var end = start + new TimeEntry(0, minutes);
                if (end >= endTime)
                    return endTime;
                return end;
            }
            
            var fullHourHeight = fullTimeHeight - fillUpMinutes * (attrs.HeightPerHour / 60f);

            int fullHours = (int) (fullHourHeight / (attrs.HeightPerHour));

            if (fullHourHeight - fullHours * attrs.HeightPerHour > (attrs.HeightPerHour / 60f) * restMinutes && fullHours * 60 + fillUpMinutes + restMinutes >= spanMinutes)
            {
                var end = start + new TimeEntry(fullHours, restMinutes + fillUpMinutes);
                if (end > endTime)
                    return endTime;
                return end;
            }
            return  start + new TimeEntry(fullHours, fillUpMinutes);
        }
        
        private TimeEntry GetEndTime(TimeEntry startTime, TimeEntry endTime)
            => endTime < startTime ? endTime + new TimeEntry(24, 0) : endTime;
    }
}
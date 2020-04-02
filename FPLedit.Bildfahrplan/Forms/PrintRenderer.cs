using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using System;
using System.Drawing;
using System.Linq;
using FPLedit.Shared.UI;
using Print = System.Drawing.Printing;

namespace FPLedit.Bildfahrplan.Forms
{
    internal sealed class PrintRenderer : IDisposable
    {
        private readonly IPluginInterface pluginInterface;
        private readonly Timetable tt;
        private readonly int route;
        private readonly TimetableStyle attrs;

        private Print.PrintDocument doc;
        private TimeEntry? last;

        public PrintRenderer(IPluginInterface pluginInterface, int route)
        {
            this.pluginInterface = pluginInterface;
            this.tt = pluginInterface.Timetable;
            this.route = route;
            attrs = new TimetableStyle(tt);
        }

        public void Dispose()
        {
            if (doc != null)
                doc.Dispose();
        }

        public void InitPrint()
        {
            var printers = Print.PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
            
            using (doc = new Print.PrintDocument())
            using (var form = new FDialog<string>())
            using (var printerDropDown = new DropDown { DataStore = printers })
            using (var paperDropDown = new DropDown())
            using (var landscapeChk = new CheckBox { Text = "Querformat" })
            using (var printButton = new Button { Text = "Drucken" })
            using (var stack = new StackLayout(printerDropDown, paperDropDown, landscapeChk, printButton) { Orientation = Orientation.Horizontal, Padding = new Eto.Drawing.Padding(10), Spacing = 5 })
            {
                form.Content = stack;
                form.DefaultButton = printButton;
                form.Title = "Bildfahrplan drucken";

                printButton.Click += (s, e) =>
                {
                    form.Result = (string)printerDropDown.SelectedValue;
                    form.Close();
                };

                printerDropDown.SelectedIndexChanged += (s, e) =>
                {
                    doc.PrinterSettings.PrinterName = (string)printerDropDown.SelectedValue;
                    var paper =  doc.PrinterSettings.PaperSizes.Cast<Print.PaperSize>().Select(p => p.PaperName).ToArray();
                    var a4Index = Array.IndexOf(paper, "A4");
                    paperDropDown.DataStore = paper;
                    paperDropDown.SelectedIndex = (a4Index != -1) ? a4Index : 0;
                };

                printerDropDown.SelectedIndex = 0;

                if (form.ShowModal() != null)
                {
                    doc.PrintPage += Doc_PrintPage;
                    doc.DocumentName = "Bildfahrplan generiert mit FPLedit";

                    doc.PrinterSettings.PrinterName = form.Result;

                    doc.DefaultPageSettings.Margins = new Print.Margins(50, 50, 50, 50); // not working (turning page????)

                    if (Eto.Platform.Instance.IsGtk)
                    {
                        var paperSize = doc.PrinterSettings.PaperSizes[paperDropDown.SelectedIndex];
                        //HACK: Do not use doc.DefaultPageSettings.Landscape on Linux, as it will lead to awkwardly turned rendering areas, ???)
                        if (landscapeChk.Checked.Value)
                            doc.DefaultPageSettings.PaperSize = new Print.PaperSize(paperSize.PaperName, paperSize.Height, paperSize.Width);
                        else
                            doc.DefaultPageSettings.PaperSize = paperSize;
                    }
                    else
                        doc.DefaultPageSettings.Landscape = landscapeChk.Checked.Value;

                    if (!doc.PrinterSettings.IsValid)
                        MessageBox.Show("Ungültige Druckereinstellungen!", "FPLedit", MessageBoxType.Error);
                    else
                    {
                        try
                        {
                            doc.Print();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Fehler beim Drucken! " + ex.Message, "FPLedit", MessageBoxType.Error);
                        }
                    }
                }
            }
        }
        private void Doc_PrintPage(object sender, Print.PrintPageEventArgs e)
        {
            // Exclude page margins to begin with
            e.Graphics.SetClip(new RectangleF(e.PageSettings.Margins.Left, e.PageSettings.Margins.Top, e.MarginBounds.Width, e.MarginBounds.Height));
            
            var renderer = new Renderer(tt, route);
            int height = e.MarginBounds.Height + e.PageSettings.Margins.Bottom + e.PageSettings.Margins.Top;
            int width = e.MarginBounds.Width + e.PageSettings.Margins.Left + e.PageSettings.Margins.Right;
            
            var margin = new Margins(e.PageSettings.Margins.Left, e.PageSettings.Margins.Top, e.PageSettings.Margins.Right, e.PageSettings.Margins.Bottom);
            renderer.SetMargins(margin);

            var start = last ?? attrs.StartTime;
            last = GetTimeByHeight(e.Graphics, renderer, start, height);

            if (Eto.Platform.Instance.IsGtk)
            {
                //HACK: Draw with the help of an extra buffer (prevent bug from System.Drawing.Commons on Linux, applying some transformation state to pages...)
                using (var bitmap = new Bitmap(width, height))
                using (var gr = Graphics.FromImage(bitmap))
                {
                    gr.PageUnit = GraphicsUnit.Display;
                    renderer.Draw(gr, start, last.Value, true, width);
                    e.Graphics.DrawImage(bitmap, 0, 0, width, height);
                }
            }
            else
                renderer.Draw(e.Graphics, start, last.Value, true, width);

            if (last.Value < attrs.EndTime)
                e.HasMorePages = true;
            else
                last = null;
        }

        //TODO: Does not work if height is not enough for at least one hour (or to next hour, see A5 landscape)
        //TODO: Remaining time < 1h is put to next page if cur+rest would fit, but cur+1h would not
        private TimeEntry GetTimeByHeight(Graphics g, Renderer renderer, TimeEntry start, int height)
        {
            var cur = start + new TimeEntry(0, (byte) (60 - start.Minutes)); // to next full hour
            var oneHour = new TimeEntry(1, 0);
            TimeEntry end = cur;
            float h = renderer.GetHeight(g, start, cur, true);
            while (true)
            {
                cur += oneHour;
                h += attrs.HeightPerHour * (oneHour.GetTotalMinutes() / 60f);
                if (h >= height)
                {
                    cur = end;
                    if (cur > attrs.EndTime)
                        return attrs.EndTime;
                    return end;
                }

                end = cur;
            }
        }
    }
}
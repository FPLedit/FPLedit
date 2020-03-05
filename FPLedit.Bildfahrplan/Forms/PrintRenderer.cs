using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Bildfahrplan.Forms
{
    internal sealed class PrintRenderer : IDisposable
    {
        private readonly IPluginInterface pluginInterface;
        private readonly Timetable tt;
        private readonly int route;
        private readonly TimetableStyle attrs;

        private PrintDocument doc;
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
            if (doc != null && !doc.IsDisposed)
                doc.Dispose();
        }

        public void InitPrint()
        {
            using (var dlg = new PrintDialog { AllowSelection = false })
            {
                if (dlg.ShowDialog((Window)pluginInterface.RootForm) == DialogResult.Ok)
                {
                    doc = new PrintDocument();
                    doc.PrintPage += Doc_PrintPage;
                    doc.Name = "Bildfahrplan generiert mit FPLedit";
                    doc.PageCount = 1;

                    doc.PrintSettings = dlg.PrintSettings;
                    doc.Print();
                }
            }
        }

        private void Doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            var renderer = new Renderer(tt, route);
            int height = (int)e.PageSize.Height;
            var start = last ?? attrs.StartTime;
            last = GetTimeByHeight(renderer, start, height);

            using (var ib = new ImageBridge(e.Graphics.ClipBounds))
            {
                renderer.Draw(ib.Graphics, start, last.Value, true);
                ib.CoptyToEto(e.Graphics);
            }

            if (last.Value < attrs.EndTime)
                doc.PageCount++;
            else
                last = null;
        }

        private TimeEntry GetTimeByHeight(Renderer renderer, TimeEntry start, int height)
        {
            var cur = start + new TimeEntry(0, (byte)(60 - start.Minutes)); // to full hour
            var oneHour = new TimeEntry(1, 0);
            TimeEntry last = cur;
            float h = renderer.GetHeight(start, cur, true);
            while (true)
            {
                cur += oneHour;
                h += attrs.HeightPerHour;
                if (h >= height)
                {
                    cur = last;
                    if (cur > attrs.EndTime)
                        return attrs.EndTime;
                    return last;
                }
                last = cur;
            }
        }


    }
}

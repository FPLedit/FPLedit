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
    internal class PrintRenderer
    {
        private IInfo info;
        private Timetable tt;
        private int route;
        private TimetableStyle attrs;
        private PrintDocument doc;

        private TimeSpan? last;

        public PrintRenderer(IInfo info, int route)
        {
            this.info = info;
            this.tt = info.Timetable;
            this.route = route;
            attrs = new TimetableStyle(tt);
        }

        public void InitPrint()
        {
            PrintDialog dlg = new PrintDialog
            {
                AllowSelection = false,
            };
            if (dlg.ShowDialog(info.RootForm) == DialogResult.Ok)
            {
                doc = new PrintDocument();
                doc.PrintPage += Doc_PrintPage;
                doc.Name = "Bildfahrplan generiert mit FPLedit";
                doc.PageCount = 1;

                doc.PrintSettings = dlg.PrintSettings;
                doc.Print();
            }
        }

        private void Doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            var renderer = new Renderer(tt, route);
            int height = (int)e.PageSize.Height;
            var start = last ?? attrs.StartTime;
            last = GetTimeByHeight(renderer, start, height);
            renderer.Draw(e.Graphics, start, last.Value);

            if (last.Value < attrs.EndTime)
                doc.PageCount++;
            else
                last = null;
        }

        private TimeSpan GetTimeByHeight(Renderer renderer, TimeSpan start, int height)
        {
            var cur = start + new TimeSpan(0, 60 - start.Minutes, 0); // to full hour
            var oneHour = new TimeSpan(1, 0, 0);
            TimeSpan last = cur;
            float h = renderer.GetHeight(start, cur);
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

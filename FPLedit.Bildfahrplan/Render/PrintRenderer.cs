using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Bildfahrplan.Render
{
    internal class PrintRenderer
    {
        private Timetable tt;
        private int route;
        private TimetableStyle attrs;

        private TimeSpan? last;

        public PrintRenderer(Timetable tt, int route)
        {
            this.tt = tt;
            this.route = route;
            attrs = new TimetableStyle(tt);
        }

        public void InitPrint()
        {
            var doc = new PrintDocument();
            doc.PrintPage += Doc_PrintPage;
            doc.DocumentName = "Bildfahrplan generiert mit FPLedit";
            PrintDialog dlg = new PrintDialog
            {
                AllowCurrentPage = false,
                AllowPrintToFile = false,
                UseEXDialog = true,
                Document = doc
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                doc.Print();
        }

        private void Doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            var renderer = new Renderer(tt, route);
            int height = e.PageBounds.Height;
            var start = last ?? attrs.StartTime;
            last = GetTimeByHeight(renderer, start, height);
            renderer.Draw(e.Graphics, start, last.Value);

            if (last.Value < attrs.EndTime)
                e.HasMorePages = true;
            else
                last = null;
        }

        private TimeSpan GetTimeByHeight(Renderer renderer, TimeSpan start, int height)
        {
            var end = start + new TimeSpan(0, 60 - start.Minutes, 0); // to full hour
            var one = new TimeSpan(1, 0, 0);
            TimeSpan last = end;
            int h = renderer.GetHeight(start, end);
            while (true)
            {
                end += one;
                h += attrs.HeightPerHour;
                if (h >= height)
                {
                    end = last;
                    var meta = attrs.EndTime;
                    if (end > meta)
                    {
                        end = meta;
                        return meta;
                    }
                    return last;
                }
                last = end;
            }
        }
    }
}

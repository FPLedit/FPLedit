using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.BuchfahrplanExport
{
    partial class BuchfahrplanTemplate
    {
        private Timetable tt;
        private string font = "\"Alte DIN 1451 Mittelschrift\"";
        private string additionalCss = "";
        private BFPL_Attrs attrs;

        public BuchfahrplanTemplate(Timetable tt)
        {
            this.tt = tt;
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");

            if (attrsEn != null)
            {
                attrs = new BFPL_Attrs(attrsEn, tt);
                font = attrs.Font;
                additionalCss = attrs.Css ?? "";
            }
        }

        private string HtmlId(string name, string prefix)
        {
            return prefix + name.Replace("#", "")
                .Replace(" ", "-")
                .Replace(".", "-")
                .Replace(":", "-")
                .ToLower();
        }

        private List<object> GetStations(TrainDirection dir)
        {
            var kms = new List<float>();
            if (attrs != null)
                kms = attrs.Points.Select(p => p.Kilometre).ToList();

            var skms = tt.Stations.Select(s => s.Kilometre).ToList();

            kms.AddRange(skms);
            var okms = kms.OrderBy(k => k);

            List<object> objs = new List<object>();
            foreach (var km in okms)
            {
                bool stationExists = skms.Contains(km);
                if (stationExists)
                {
                    Station sta = tt.Stations.First(s => s.Kilometre == km);
                    objs.Add(sta);
                }
                else if (attrs != null)
                {
                    BFPL_Point point = attrs.Points.First(p => p.Kilometre == km);
                    objs.Add(point);
                }
            }

            Func<object, float> order = o =>
            {
                float km = -1;
                if (o.GetType() == typeof(Station))
                    km = ((Station)o).Kilometre;
                else if (o.GetType() == typeof(BFPL_Point))
                    km = ((BFPL_Point)o).Kilometre;
                return km;
            };

            return (dir.Get() ?
                objs.OrderByDescending(order)
                : objs.OrderBy(order)).ToList();
        }
    }
}

using FPLedit.BuchfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.BuchfahrplanExport.Templates
{
    public class TemplateHelper
    {
        public BFPL_Attrs Attrs { get; set; }
        public Timetable TT { get; set; }

        public string HtmlName(string name, string prefix)
        {
            return prefix + name.Replace("#", "")
                .Replace(" ", "-")
                .Replace(".", "-")
                .Replace(":", "-")
                .ToLower();
        }

        public List<Entity> GetStations(TrainDirection dir)
        {
            var kms = new List<float>();
            if (Attrs != null)
                kms = Attrs.Points.Select(p => p.Kilometre).ToList();

            var skms = TT.Stations.Select(s => s.Kilometre).ToList();

            kms.AddRange(skms);
            var okms = kms.OrderBy(k => k);

            List<Entity> objs = new List<Entity>();
            foreach (var km in okms)
            {
                bool stationExists = skms.Contains(km);
                if (stationExists)
                {
                    Station sta = TT.Stations.First(s => s.Kilometre == km);
                    objs.Add(sta);
                }
                else if (Attrs != null)
                {
                    BFPL_Point point = Attrs.Points.First(p => p.Kilometre == km);
                    objs.Add(point);
                }
            }

            Func<Entity, float> order = o =>
            {
                float km = -1;
                if (o.GetType() == typeof(Station))
                    km = ((Station)o).Kilometre;
                else if (o.GetType() == typeof(BFPL_Point))
                    km = ((BFPL_Point)o).Kilometre;
                return km;
            };

            return (dir == TrainDirection.ta ?
                objs.OrderByDescending(order)
                : objs.OrderBy(order)).ToList();
        }
    }
}

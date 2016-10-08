using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FPLedit.BuchfahrplanExport
{
    public class HtmlExport : IExport
    {
        private Template template;

        public string Filter
        {
            get { return "Buchfahrplan als HTML Datei (*.html)|*.html"; }
        }

        public bool Reoppenable
        {
            get { return false; }
        }

        public HtmlExport()
        {
            template = new Template(LoadResource("Templates.Main"), LoadResource("Templates.Line"), LoadResource("Templates.Train"));
        }

        private string LoadResource(string filename)
        {
            var assembly = Assembly.GetAssembly(GetType());
            var resourceName = "FPLedit.BuchfahrplanExport." + filename;


            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        public bool Export(Timetable timetable, string filename, ILog logger)
        {
            string cont = template.GlobalTemplate.Replace("{0}", BuildTrain(timetable));
            File.WriteAllText(filename, cont);
            return true;
        }

        private string BuildTrain(Timetable tt)
        {
            string res = "";
            foreach (var t in tt.Trains)
            {
                string val = "";
                foreach (var s in tt.Stations.OrderBy(o => o.Kilometre))
                {
                    val += template.LineTemplate
                        .Replace("{0}", s.Kilometre.ToString("0.0"))
                        .Replace("{1}", s.GetMetaInt("MaxVelocity", 0).ToString("#")).Replace("{2}", s.Name)
                        .Replace("{3}", t.Arrivals.ContainsKey(s) ? t.Arrivals[s].ToShortTimeString() : "")
                        .Replace("{4}", t.Departures.ContainsKey(s) ? t.Departures[s].ToShortTimeString() : "");
                }

                res += template.TrainTemplate
                    .Replace("{0}", t.Name)
                    .Replace("{1}", t.Line)
                    .Replace("{2}", "Tfz " + t.Locomotive)
                    .Replace("{3}", val);
            }
            return res;
        }

        private class Template
        {
            public string GlobalTemplate { get; private set; }

            public string TrainTemplate { get; private set; }

            public string LineTemplate { get; private set; }

            public Template(string glob, string line, string train)
            {
                GlobalTemplate = glob;
                TrainTemplate = train;
                LineTemplate = line;
            }
        }
    }
}

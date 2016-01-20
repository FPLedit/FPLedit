using Buchfahrplan.FileModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan.Export
{
    public class HtmlExport : IExport
    {
        private Template template;
        public HtmlExport()
        {
            template = new Template(Decompress("output.tmpl"));
        }
        //public readonly string GLOB_TEMPLATE = "<!DOCTYPE html><html><head><style>.tfz{padding-top:1px;padding-right:1px;padding-left:1px;color:black;font-size:11.0pt;font-weight:400;font-style:normal;text-decoration:none;font-family:\"DIN 1451 Mittelschrift Alt\", sans-serif;text-align:general;vertical-align:bottom;white-space:nowrap;}table{border-collapse: collapse;table-layout:fixed;width:454pt;}.trainname{height:19.5pt;padding-top:1px;padding-right:1px;padding-left:1px;color:black;font-size:15.0pt;font-weight:400;font-style:normal;text-decoration:none;font-family:\"DIN 1451 Mittelschrift Alt\", sans-serif;text-align:center;vertical-align:middle;white-space:nowrap;}.linename{padding-top:1px;padding-right:1px;padding-left:1px;color:black;font-size:12.0pt;font-weight:400;font-style:normal;text-decoration:none;font-family:\"DIN 1451 Mittelschrift Alt\", sans-serif;text-align:center;vertical-align:middle;white-space:nowrap;}.spaltennummer{padding-top:1px;padding-right:1px;padding-left:1px;color:black;font-size:11.0pt;font-weight:400;font-style:normal;text-decoration:none;font-family:\"DIN 1451 Mittelschrift Alt\", sans-serif;text-align:center;vertical-align:top;border-top:1.5pt solid windowtext;border-right:1.5pt solid windowtext;border-bottom:.5pt solid windowtext;border-left:1.5pt solid windowtext;white-space:nowrap;}.spaltenkopf{padding-top:1px;padding-right:1px;padding-left:1px;color:black;font-size:11.0pt;font-weight:400;font-style:normal;text-decoration:none;font-family:\"DIN 1451 Mittelschrift Alt\", sans-serif;text-align:center;vertical-align:top;border-top:.5pt solid windowtext;border-right:1.5pt solid windowtext;border-bottom:1.5pt solid windowtext;border-left:1.5pt solid windowtext;white-space:normal;}.zug{padding-top:1px;padding-right:1px;padding-left:1px;color:black;font-size:11.0pt;font-weight:400;font-style:normal;text-decoration:none;font-family:\"DIN 1451 Mittelschrift Alt\", sans-serif;text-align:center;vertical-align:top;border-top:none;border-right:1.5pt solid windowtext;border-bottom:none;border-left:1.5pt solid windowtext;white-space:nowrap;}.tabellenende{padding-top:1px;padding-right:1px;padding-left:1px;color:black;font-size:11.0pt;font-weight:400;font-style:normal;text-decoration:none;font-family:\"DIN 1451 Mittelschrift Alt\", sans-serif;text-align:center;vertical-align:top;border-top:none;border-right:1.5pt solid windowtext;border-bottom:1.5pt solid windowtext;border-left:1.5pt solid windowtext;white-space:nowrap;}</style></head><body><div><table> <col width=80 span=2> <col width=285> <col width=80 span=2>{0}</table></div></body></html>";

        public void Export(FileModel.Timetable timetable, string filename)
        {
            string cont = template.GlobalTemplate.Replace("{0}", BuildTrain(timetable));
            File.WriteAllText(filename, cont);  
        }

        //public readonly string TRAIN_TEMPLATE = "<tr><td colspan=5 class=trainname>{0}</td></tr><tr><td colspan=5 class=linename>{1}</td></tr><tr><td class=tfz>{2}</td><td></td><td></td><td></td><td></td></tr><tr><td class=spaltennummer>0</td><td class=spaltennummer>1</td><td class=spaltennummer>2</td><td class=spaltennummer>3</td><td class=spaltennummer>4</td></tr><tr height=133> <td class=spaltenkopf>Lage<br>der<br>Betriebs-<br>stelle<br><br>(km)</td><td class=spaltenkopf>Höchst-<br>Geschwin-<br>digkeit<br><br><br>(km/h)</td><td class=spaltenkopf>Betriebsstellen,<br>ständige Langsamfahrstellen,<br>verkürzter Vorsignalabstand</td><td class=spaltenkopf>Ankunft</td><td class=spaltenkopf>Abfahrt<br>oder Durch-<br>fahrt</td></tr>{3}<tr height=21><td class=tabellenende></td><td class=tabellenende></td><td class=tabellenende></td><td class=tabellenende></td><td class=tabellenende></td></tr><tr height=21><td></td><td></td><td></td><td></td><td></td></tr><tr height=21><td></td><td></td><td></td><td></td><td></td></tr><tr height=21><td></td><td></td><td></td><td></td><td></td></tr>";
        //public readonly string TRAIN_TIME_LINE = "<tr height=21><td class=zug>{0}</td><td class=zug>{1}</td><td class=zug>{2}</td><td class=zug>{3}</td><td class=zug>{4}</td></tr>";
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
                        .Replace("{1}", s.MaxVelocity.ToString("#")).Replace("{2}", s.Name)
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

        public Dictionary<string, string> Decompress(string filename)
        {
            string sep = "////////////";
            string cont = File.ReadAllText(filename);
            string file = "";
            string filecont = "";
            Dictionary<string, string> files = new Dictionary<string, string>();

            using (StringReader sr = new StringReader(cont))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith(sep))
                    {
                        if (file != "")
                        {
                            files.Add(file, filecont);
                            file = "";
                            filecont = "";
                        }
                        file = line.Trim('/');
                    }
                    else
                    {
                        filecont += line;
                    }
                }
                if (file != "")                
                    files.Add(file, filecont);
            }

            return files;
        }
    }

    public class Template
    {
        public string GlobalTemplate { get; private set; }

        public string TrainTemplate { get; private set; }

        public string LineTemplate { get; private set; }

        public Template(Dictionary<string, string> filetable)
        {
            GlobalTemplate = filetable["GLOB_TEMPLATE"];
            TrainTemplate = filetable["TRAIN_TEMPLATE"];
            LineTemplate = filetable["LINE_TEMPLATE"];
        }
    }
}

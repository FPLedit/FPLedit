using Buchfahrplan.Shared;
using System;

namespace Buchfahrplan.AushangfahrplanHtmlExport
{
    public class AushangfahrplanHtmlExport : IExport
    {
        public string Filter
        {
            get
            {
                return "Aushangfahrplan als HTML Datei (*.html)|*.html";
            }
        }

        public bool Reoppenable
        {
            get
            {
                return false;
            }
        }

        public bool Export(Timetable tt, string filename, ILog logger)
        {
            throw new NotImplementedException();
        }
    }
}

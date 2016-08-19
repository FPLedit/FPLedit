using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buchfahrplan.Standard
{
    public class BfplExport : IExport
    {
        public string Filter
        {
            get
            {
                return "Buchfahrplan Datei (*.bfpl)|*bfpl";
            }
        }

        public bool Reoppenable
        {
            get
            {
                return true;
            }
        }

        public bool Export(Timetable tt, string filename, ILog logger)
        {
            try
            {
                tt.SaveToFile(filename);
                return true;
            }
            catch (Exception ex)
            {
                logger.Log("BfplExport: Error: " + ex.Message);
                return false;
            }
        }
    }
}

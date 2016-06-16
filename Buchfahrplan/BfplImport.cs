using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buchfahrplan
{
    public class BfplImport : IImport
    {
        public string Filter
        {
            get
            {
                return "Buchfahrplan Dateien (*.bfpl)|*.bfpl";
            }
        }

        public Timetable Import(string filename, ILog logger)
        {
            try
            {
                return Timetable.OpenFromFile(filename);
            }
            catch (Exception ex)
            {
                logger.Log("BfplImport: Error: " + ex.Message);
                return null;
            }
        }
    }
}

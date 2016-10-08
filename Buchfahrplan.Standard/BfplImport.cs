using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Standard
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
                logger.Error("BfplImport: " + ex.Message);
                return null;
            }
        }
    }
}

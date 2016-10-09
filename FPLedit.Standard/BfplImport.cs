using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
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
                return "FPLedit Dateien (*.bfpl)|*.bfpl";
            }
        }

        public Timetable Import(string filename, ILog logger)
        {
            try
            {
                using (FileStream stream = File.Open(filename, FileMode.Open))
                {
                    return Timetable.OpenFromStream(stream);
                }
            }
            catch (Exception ex)
            {
                logger.Error("BfplImport: " + ex.Message);
                return null;
            }
        }
    }
}

using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FPLedit.Shared.Filetypes
{
    public class XMLImport : IImport
    {
        public string Filter
        {
            get
            {
                return "Fahrplan Dateien (*.fpl)|*.fpl";
            }
        }

        public Timetable Import(string filename, ILog logger)
        {
            try
            {
                XElement el = XElement.Load(filename);

                XMLEntity en = new XMLEntity(el);
                return new Timetable(en);
        }
            catch (Exception ex)
            {
                logger.Error("JTrainGraphImporter: " + ex.Message);
                return null;
            }
}
    }
}

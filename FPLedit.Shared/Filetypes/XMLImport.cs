using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FPLedit.Shared.Filetypes
{
    public class XMLImport : IImport
    {
        public string Filter => "Fahrplan Dateien (*.fpl)|*.fpl";

        public Timetable Import(string filename, ILog logger)
        {
            try
            {
                XElement el = XElement.Load(filename);

                XMLEntity en = new XMLEntity(el);
                bool isNetwork = en.GetAttribute("version", "") == "100";
                return new Timetable(en, isNetwork ? TimetableType.Network : TimetableType.Linear);
            }
            catch (Exception ex)
            {
                logger.Error("XMLImporter: " + ex.Message);
                return null;
            }
        }
    }
}

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

        public Timetable Import(string filename, IInfo info, ILog replaceLog = null)
        {
            try
            {
                XElement el = XElement.Load(filename);

                XMLEntity en = new XMLEntity(el);
                var tt = new Timetable(en);

                var actions = info.GetRegistered<ITimetableInitAction>();
                foreach (var action in actions)
                    action.Init(tt);

                return tt;
            }
            catch (Exception ex)
            {
                var log = replaceLog ?? info.Logger;
                log.Error("XMLImporter: " + ex.Message);
                return null;
            }
        }
    }
}

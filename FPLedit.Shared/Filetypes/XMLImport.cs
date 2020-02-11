using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FPLedit.Shared.Filetypes
{
    public sealed class XMLImport : IImport
    {
        public string Filter => "Fahrplan Dateien (*.fpl)|*.fpl";

        public Timetable Import(Stream stream, IPluginInterface pluginInterface, ILog replaceLog = null)
        {
            try
            {
                XElement el = XElement.Load(stream);

                XMLEntity en = new XMLEntity(el);
                var tt = new Timetable(en);

                var actions = pluginInterface.GetRegistered<ITimetableInitAction>();
                foreach (var action in actions)
                {
                    var message = action.Init(tt);
                    if (message != null)
                        pluginInterface.Logger.Warning(message);
                }

                return tt;
            }
            catch (Exception ex)
            {
                var log = replaceLog ?? pluginInterface.Logger;
                log.Error("XMLImporter: " + ex.Message);
                return null;
            }
        }
    }
}

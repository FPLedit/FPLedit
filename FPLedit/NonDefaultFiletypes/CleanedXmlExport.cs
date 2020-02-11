using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FPLedit.NonDefaultFiletypes
{
    internal sealed class CleanedXmlExport : IExport
    {
        public string Filter => "Bereinigte Fahrplan Dateien (*.fpl)|*.fpl";

        private readonly string[] node_names = new[]
        {
            "bfpl_attrs",   // Buchfahrplaneigenschaften
            "afpl_attrs",   // Aushangfahrplaneigenschaften
            "kfpl_attrs",   // Kursbucheigenschaften
        };

        //TODO: Better method to remove all known attributes? (yes, use attributes)
        private readonly string[] attrs_names = new[]
        {
            "fpl-vmax",     // Höchstgeschwindigkeit
            "fpl-wl",       // Wellenlinien
            "fpl-tr",       // Trapeztafel
            "fpl-zlm",      // Zuglaufmeldung
            "fpl-tfz",      // Triebfahrzeug
            "fpl-mbr",      // Mindestbremshundertstel
            "fpl-last",     // max. Last eines Zuges
        };

        private XElement BuildNode(XMLEntity node)
        {
            XElement elm = new XElement(node.XName);
            if (node.Value != null)
                elm.SetValue(node.Value);

            var fAttrs = node.Attributes.Where(a => !attrs_names.Contains(a.Key));
            foreach (var attr in fAttrs)
                elm.SetAttributeValue(attr.Key, attr.Value);

            var f_nodes = node.Children.Where(c => !node_names.Contains(c.XName));
            foreach (var ch in f_nodes)
                elm.Add(BuildNode(ch));
            return elm;
        }

        public bool Export(Timetable tt, Stream stream, IPluginInterface pluginInterface, string[] flags = null)
        {
            if (pluginInterface.Timetable.Type == TimetableType.Network)
            {
                MessageBox.Show("Der aktuelle Fahrplan ist ein Netzwerk-Fahrplan. Aus diesem erweiterten Fahrplanformat können aus technischen Gründen nicht alle von FPLedit angelegten Daten gelöscht werden.");
                return false;
            }

            var res = MessageBox.Show("Hiermit werden alle in FPLedit zusätzlich eingebenen Werte (z.B. Lokomotiven, Lasten, Mindestbremshundertstel, Geschwindigkeiten, Wellenlinien, Trapeztafelhalte und Zuglaufmeldungen) und Buchfahrplaneinstellungen aus dem gespeicherten Fahrplan gelöscht! Fortfahren?",
                "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning);

            if (res == DialogResult.No)
                return false;

            bool debug = pluginInterface.Settings.Get<bool>("xml.indent");
#if DEBUG
            debug = true;
#endif
            try
            {
                var clone = tt.Clone(); // Klon zum anschließenden Verwerfen!
                var ttElm = BuildNode(clone.XMLEntity);

                using (var writer = new XmlTextWriter(stream, new UTF8Encoding(false)))
                {
                    if (debug)
                        writer.Formatting = Formatting.Indented;
                    ttElm.Save(writer);
                }
                return true;
            }
            catch (Exception ex)
            {
                pluginInterface.Logger.Error("XMLExport: " + ex.Message);
                return false;
            }
        }
        
        public bool Export(Timetable tt, string filename, IPluginInterface pluginInterface, string[] flags = null)
        {
            using (var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.SetLength(0);
                return Export(tt, stream, pluginInterface, flags);
            }
        }
    }
}

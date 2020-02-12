using Eto.Forms;
using FPLedit.Shared;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FPLedit.Shared.Filetypes;

namespace FPLedit.NonDefaultFiletypes
{
    internal sealed class CleanedXmlExport : IExport
    {
        public string Filter => "Bereinigte Fahrplan Dateien (*.fpl)|*.fpl";

        private readonly string[] nodeNames =
        {
            "bfpl_attrs",   // Buchfahrplaneigenschaften
            "afpl_attrs",   // Aushangfahrplaneigenschaften
            "kfpl_attrs",   // Kursbucheigenschaften
        };

        //TODO: Better method to remove all known attributes? (yes, use attributes)
        private readonly string[] attrsNames =
        {
            "fpl-vmax",     // Höchstgeschwindigkeit
            "fpl-wl",       // Wellenlinien
            "fpl-tr",       // Trapeztafel
            "fpl-zlm",      // Zuglaufmeldung
            "fpl-tfz",      // Triebfahrzeug
            "fpl-mbr",      // Mindestbremshundertstel
            "fpl-last",     // max. Last eines Zuges
        };

        private void ProcessEntity(XMLEntity node)
        {
            foreach (var attr in attrsNames)
                node.RemoveAttribute(attr);

            node.Children.RemoveAll(x => nodeNames.Contains(x.XName));

            foreach (var ch in node.Children)
                ProcessEntity(ch);
        }

        public bool Export(Timetable tt, Stream stream, IPluginInterface pluginInterface, string[] flags = null)
        {
            if (pluginInterface.Timetable.Type == TimetableType.Network)
            {
                MessageBox.Show("Der aktuelle Fahrplan ist ein Netzwerk-Fahrplan. Aus diesem erweiterten Fahrplanformat können aus technischen Gründen keine von FPLedit angelegten Daten gelöscht werden.");
                return false;
            }

            var res = MessageBox.Show("Hiermit werden alle in FPLedit zusätzlich eingebenen Werte (z.B. Lokomotiven, Lasten, Mindestbremshundertstel, Geschwindigkeiten, Wellenlinien, Trapeztafelhalte und Zuglaufmeldungen) und Buchfahrplaneinstellungen aus dem gespeicherten Fahrplan gelöscht! Fortfahren?",
                "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning);

            if (res == DialogResult.No)
                return false;

            var clone = tt.Clone(); // Klon zum anschließenden Verwerfen!
            ProcessEntity(clone.XMLEntity);

            return new XMLExport().Export(clone, stream, pluginInterface, flags);
        }
    }
}

using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FPLedit.Shared.Filetypes
{
    public class XMLExport : IExport
    {
        public string Filter
        {
            get
            {
                return "Fahrplan Dateien (*.fpl)|*.fpl";
            }
        }

        private XElement BuildNode(XMLEntity node)
        {
            XElement elm = new XElement(node.XName);
            if (node.Value != null)
                elm.SetValue(node.Value);
            foreach (var attr in node.Attributes)
                elm.SetAttributeValue(attr.Key, attr.Value);
            foreach (var ch in node.Children)
                elm.Add(BuildNode(ch));
            return elm;
        }

        public bool Export(Timetable tt, string filename, ILog logger)
        {
            bool debug = bool.Parse(SettingsManager.Get("xml.indent", "False"));
#if DEBUG
            debug = true;
#endif
            try
            {
                var ttElm = BuildNode(tt.XMLEntity);

                using (var writer = new XmlTextWriter(filename, new UTF8Encoding(false)))
                {
                    if (debug)
                        writer.Formatting = Formatting.Indented;
                    ttElm.Save(writer);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("XMLExport: " + ex.Message);
                return false;
            }
        }
    }
}

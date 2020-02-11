using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FPLedit.Shared.Filetypes
{
    public sealed class XMLExport : IExport
    {
        public const string FLAG_INDENT_XML = "indent_xml";
        
        public string Filter => "Fahrplan Dateien (*.fpl)|*.fpl";
        
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

        public bool Export(Timetable tt, string filename, IPluginInterface pluginInterface, string[] flags = null)
        {
            using (var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.SetLength(0);
                return Export(tt, stream, pluginInterface, flags);
            }
        }
        
        public bool Export(Timetable tt, Stream stream, IPluginInterface pluginInterface, string[] flags = null)
        {
            bool debug = pluginInterface.Settings.Get<bool>("xml.indent") || (flags?.Contains(FLAG_INDENT_XML) ?? false);
#if DEBUG
            debug = true;
#endif
            try
            {
                var ttElm = BuildNode(tt.XMLEntity);

                using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                using (var writer = new XmlTextWriter(sw))
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
    }
}

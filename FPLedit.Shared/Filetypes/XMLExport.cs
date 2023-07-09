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
        
        public string Filter => T._("Fahrplan Dateien (*.fpl)|*.fpl");
        
        private XElement BuildNode(XMLEntity node)
        {
            var xElement = new XElement(node.XName);
            if (node.Value != null)
                xElement.SetValue(node.Value);
            foreach (var attr in node.Attributes)
                xElement.SetAttributeValue(attr.Key, attr.Value);
            foreach (var ch in node.Children)
                xElement.Add(BuildNode(ch));
            return xElement;
        }
        
        public bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null) 
            => ExportGenericNode(tt.XMLEntity, stream, pluginInterface, flags);

        public bool ExportGenericNode(XMLEntity xmlEntity, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null)
        {
            bool debug = pluginInterface.Settings.Get<bool>("xml.indent") || (flags?.Contains(FLAG_INDENT_XML) ?? false);
#if DEBUG
            debug = true;
#endif
            
            var ttElm = BuildNode(xmlEntity);

            using var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true);
            using var writer = new XmlTextWriter(sw);
            if (debug)
                writer.Formatting = Formatting.Indented;
            ttElm.Save(writer);
            return true;
        }
    }
}

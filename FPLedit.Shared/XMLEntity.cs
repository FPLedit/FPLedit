using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    [DebuggerDisplay("Name: {XName}, Children: {Children.Count}")]
    public sealed class XMLEntity : IEntity
    {
        public string XName { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public List<XMLEntity> Children { get; set; }

        public string Value { get; set; }

        public XMLEntity(string xname)
        {
            XName = xname;
            Attributes = new Dictionary<string, string>();
            Children = new List<XMLEntity>();
        }

        public XMLEntity(XElement el)
        {
            if (el.Name.Namespace != XNamespace.None)
                throw new NotSupportedException("Dateien mit XML-Namensräumen werden nicht unterstützt!");
            XName = el.Name.LocalName;
            if (!el.Attributes().All(a => a.Name.Namespace == XNamespace.None))
                throw new NotSupportedException("Dateien mit XML-Namensräumen werden nicht unterstützt!");
            Attributes = el.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);
            Children = new List<XMLEntity>();
            Value = el.Nodes().OfType<XText>().FirstOrDefault()?.Value;
            foreach (var c in el.Nodes().OfType<XElement>())
                Children.Add(new XMLEntity(c));
        }

        public T GetAttribute<T>(string key, T defaultValue = default(T))
        {
            if (Attributes.ContainsKey(key))
                return (T)Convert.ChangeType(Attributes[key], typeof(T));
            return defaultValue;
        }

        public void SetAttribute(string key, string value)
        {
            Attributes[key] = value;
        }

        public void RemoveAttribute(string key)
        {
            Attributes.Remove(key);
        }
    }
}

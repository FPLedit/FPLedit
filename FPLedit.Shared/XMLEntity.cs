using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    public class XMLEntity
    {
        [NonSerialized]
        public XElement el;

        public Timetable _parent;

        public string XName { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public List<XMLEntity> Children { get; set; }

        public XMLEntity(string xname, Timetable tt)
        {
            XName = xname;
            _parent = tt;
            Attributes = new Dictionary<string, string>();
            Children = new List<XMLEntity>();
        }

        public XMLEntity(XElement el, Timetable tt)
        {
            this.el = el;
            _parent = tt;
            XName = el.Name.LocalName;
            Attributes = el.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);
            Children = new List<XMLEntity>();
            foreach (var c in el.Elements())
                Children.Add(new XMLEntity(c, tt));
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
    }
}

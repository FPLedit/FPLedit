using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// Type of the XML tree, used as a base for the object model. Represents a single XML node.
    /// </summary>
    [DebuggerDisplay("Name: {XName}, Children: {Children.Count}, Attrs: {AttributeDebugger,nq}")]
    [Templating.TemplateSafe]
    public sealed class XMLEntity
    {
        /// <summary>
        /// Name of the XML node. Naming rules apply.
        /// </summary>
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
            if (el.Attributes().Any(a => a.Name.Namespace != XNamespace.None))
                throw new NotSupportedException("Dateien mit XML-Namensräumen werden nicht unterstützt!");
            Attributes = el.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);
            Children = new List<XMLEntity>();
            Value = el.Nodes().OfType<XText>().FirstOrDefault()?.Value;
            foreach (var c in el.Nodes().OfType<XElement>())
                Children.Add(new XMLEntity(c));
        }

        public T GetAttribute<T>(string key, T defaultValue = default)
        {
            if (Attributes.TryGetValue(key, out string val))
            {
                if (val == "" && typeof(T) != typeof(string))
                    return defaultValue;
                return (T)Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
            }
            return defaultValue;
        }

        public void SetAttribute(string key, string value) => Attributes[key] = value;

        public void RemoveAttribute(string key) => Attributes.Remove(key);

        private string AttributeDebugger => string.Join(", ", Attributes.ToList().Select(a => a.Key + "=" + a.Value));
        
        /// <summary>
        /// Returns if this entity is different on the top level or any child level, from the given other entity. This is a deep-compare operation.
        /// </summary>
        /// <returns>True, if both XMLentities are "equal".</returns>
        public bool XDiff(XMLEntity other)
        {
            if (XName != other.XName)
                return false;
            if (Value != other.Value)
                return false;
            if (Attributes.Count != other.Attributes.Count)
                return false;
            if (Attributes.Any(a => a.Value != other.GetAttribute<string>(a.Key, null)))
                return false;
            if (Children.Count != other.Children.Count)
                return false;
            return Children.All(c => c.XDiff(other.Children[Children.IndexOf(c)]));
        }

        /// <summary>
        /// Create a copy of this XML entities' XML tree.
        /// </summary>
        public XMLEntity XClone()
        {
            var clone = new XMLEntity(XName);
            foreach (var attr in Attributes)
                clone.SetAttribute(attr.Key, attr.Value);
            foreach (var child in Children)
                clone.Children.Add(child.XClone());

            return clone;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class XElmNameAttribute : Attribute
    {
        public XElmNameAttribute(params string[] names)
        {
            Names = names;
        }

        public string[] Names { get; }

        public bool IsFpleditElement { get; set; }

        public string[] ParentElements { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class XAttrNameAttribute : Attribute
    {
        public XAttrNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
        
        public bool IsFpleditElement { get; set; }
    }
}

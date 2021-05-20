using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    // ReSharper disable once InconsistentNaming
    public sealed class XMLEntity
    {
        private XMLEntity? ParentElement { get; set; }
        
        private readonly ObservableCollection<XMLEntity> children;

        /// <summary>
        /// Name of the XML node. Naming rules apply.
        /// </summary>
        public string XName { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public IList<XMLEntity> Children => children;

        public string? Value { get; set; }

        /// <summary>
        /// This event will be called when one of the children of this element (or any element in the XML subtree below)
        /// is changed (attributes and/or children elements).
        /// </summary>
        public event EventHandler? ChildrenChangedRecursive;
        
        /// <summary>
        /// This event will beraised when the children collection of this entity will be modified.
        /// </summary>
        public event EventHandler? ChildrenChangedDirect;

        public XMLEntity(string xname)
        {
            XName = xname;
            Attributes = new Dictionary<string, string>();
            children = new ObservableCollection<XMLEntity>();
            children.CollectionChanged += ChildrenOnCollectionChanged;
        }

        public XMLEntity(XElement el)
        {
            if (el.Name.Namespace != XNamespace.None)
                throw new NotSupportedException("Files witth XML namespaces are not supported!");
            XName = el.Name.LocalName;
            if (el.Attributes().Any(a => a.Name.Namespace != XNamespace.None))
                throw new NotSupportedException("Files witth XML namespaces are not supported!");
            Attributes = el.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);
            Value = el.Nodes().OfType<XText>().FirstOrDefault()?.Value;
            
            children = new ObservableCollection<XMLEntity>();
            foreach (var c in el.Nodes().OfType<XElement>())
            {
                var xEntity = new XMLEntity(c);
                xEntity.ParentElement = this;
                children.Add(xEntity);
            }

            children.CollectionChanged += ChildrenOnCollectionChanged;
        }

        private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (XMLEntity? item in e.NewItems)
                    item!.ParentElement = this;
            
            ChildrenChangedRecursive?.Invoke(this, e);
            ChildrenChangedDirect?.Invoke(this, e);
            ParentElement?.NotifyChildChanged();
        }

        private void NotifyChildChanged()
        {
            ChildrenChangedRecursive?.Invoke(this, new EventArgs());
            ParentElement?.NotifyChildChanged();
        }
        
        [return: MaybeNull]
        [return: NotNullIfNotNull("defaultValue")]
        public T GetAttribute<T>(string key, [AllowNull] T defaultValue = default)
        {
            if (Attributes.TryGetValue(key, out string? val))
            {
                if (val == "" && typeof(T) != typeof(string))
                    return defaultValue;
                return (T)Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
            }
            return defaultValue;
        }

        public void SetAttribute(string key, string value)
        {
            Attributes[key] = value;
            ParentElement?.NotifyChildChanged();
        }

        public void RemoveAttribute(string key)
        {
            Attributes.Remove(key);
            ParentElement?.NotifyChildChanged();
        }

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
            if (Attributes.Any(a => a.Value != other.GetAttribute<string>(a.Key)))
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
            ParentElements = Array.Empty<string>();
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

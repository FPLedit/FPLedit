using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [Serializable]
    public class Entity
    {
        public XMLEntity XMLEntity { get; private set; }

        public Timetable _parent { get; set; }

        public string XName
        {
            get => XMLEntity.XName;
            set => XMLEntity.XName = value;
        }

        public Dictionary<string, string> Attributes
        {
            get => XMLEntity.Attributes;
            set => XMLEntity.Attributes = value;
        }

        public List<XMLEntity> Children
        {
            get => XMLEntity.Children;
            set => XMLEntity.Children = value;
        }

        public Entity(string xn, Timetable tt)
        {
            XMLEntity = new XMLEntity(xn);
            _parent = tt;
        }

        public Entity(XMLEntity en, Timetable tt)
        {
            XMLEntity = en;
            _parent = tt;
        }

        public T GetAttribute<T>(string key, T defaultValue = default(T))
            => XMLEntity.GetAttribute<T>(key, defaultValue);

        public void SetAttribute(string key, string value)
            => XMLEntity.SetAttribute(key, value);

        public void RemoveAttribute(string key)
            => XMLEntity.RemoveAttribute(key);
    }
}

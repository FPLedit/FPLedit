using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace FPLedit.Shared
{
    [Serializable]
    public abstract class Entity : IEntity
    {
        public XMLEntity XMLEntity { get; private set; }

        public Timetable _parent { get; set; }

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

        public T GetAttribute<T>(string key, T defaultValue = default)
            => XMLEntity.GetAttribute<T>(key, defaultValue);

        public void SetAttribute(string key, string value)
            => XMLEntity.SetAttribute(key, value);

        public void RemoveAttribute(string key)
            => XMLEntity.RemoveAttribute(key);

        public T Clone<T>() where T : Entity
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        protected void SetNotEmptyTime(TimeSpan time, string key)
        {
            var t = time.ToShortTimeString();
            SetAttribute(key, t != "00:00" ? t : "");
        }

        protected TimeSpan GetTimeValue(string key)
        {
            var val = GetAttribute(key, "");
            TimeSpan.TryParse(val, out var ts);
            return ts;
        }
    }
}

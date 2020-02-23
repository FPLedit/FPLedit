using System;
using System.Collections.Generic;
using Force.DeepCloner;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public abstract class Entity : IEntity
    {
        public XMLEntity XMLEntity { get; }

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

        protected Entity(string xn, Timetable tt)
        {
            XMLEntity = new XMLEntity(xn);
            _parent = tt;
        }

        protected Entity(XMLEntity en, Timetable tt)
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

        public T Clone<T>() where T : Entity => (T)this.DeepClone();

        protected void SetNotEmptyTime(TimeEntry time, string key)
        {
            var t = time.ToShortTimeString();
            SetAttribute(key, t != "00:00" ? t : "");
        }

        protected TimeEntry GetTimeValue(string key)
        {
            var val = GetAttribute(key, "");
            TimeEntry.TryParse(val, out var ts);
            return ts;
        }
    }
}

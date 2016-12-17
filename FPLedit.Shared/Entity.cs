using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [Serializable]
    public abstract class Entity
    {
        public Dictionary<string, string> Attributes { get; set; }

        public Entity()
        {
            Attributes = new Dictionary<string, string>();
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

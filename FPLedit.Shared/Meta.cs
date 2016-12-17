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
            //Metadata = new Dictionary<string, string>();
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

        /*[Obsolete]
        public Dictionary<string, string> Metadata { get; set; }

        [Obsolete]
        public void SetMeta(string key, string value)
        {
            Metadata[key] = value;
        }

        [Obsolete]
        public string GetMeta(string key, string defaultValue)
        {
            if (Metadata.ContainsKey(key))
                return Metadata[key];
            else
                return defaultValue;
        }

        [Obsolete]
        public T GetMeta<T>(string key, T defaultValue, Func<string, T> func)
        {
            if (Metadata.ContainsKey(key))
                return func(Metadata[key]);
            else
                return defaultValue;
        }

        public int GetMetaInt(string key, int defaultValue)
        {
            return GetMeta(key, defaultValue, int.Parse);
        }

        public float GetMetaFloat(string key, float defaultValue)
        {
            return GetMeta(key, defaultValue, float.Parse);
        }

        public bool GetMetaBool(string key, bool defaultValue)
        {
            return GetMeta(key, defaultValue, bool.Parse);
        }

        public Color GetMetaColor(string key, Color defaultValue)
        {
            return GetMeta(key, defaultValue, ColorHelper.ColorFromHex);
        }

        public TimeSpan GetMetaTimeSpan(string key, TimeSpan defaultValue)
        {
            return GetMeta(key, defaultValue, TimeSpan.Parse);
        }*/
    }
}

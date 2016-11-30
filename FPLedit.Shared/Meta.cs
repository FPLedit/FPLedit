using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [Serializable]
    public abstract class Meta
    {
        public Dictionary<string, string> Metadata { get; set; }

        public Meta()
        {
            Metadata = new Dictionary<string, string>();
        }

        public string GetMeta(string key, string defaultValue)
        {
            if (Metadata.ContainsKey(key))
                return Metadata[key];
            else
                return defaultValue;
        }

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
        }

        public static Dictionary<string, string> DeserializeMeta(BinaryReader reader)
        {
            var res = new Dictionary<string, string>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                string value = reader.ReadString();
                res.Add(key, value);
            }
            return res;
        }

        public void SerializeMeta(BinaryWriter writer)
        {
            writer.Write(Metadata.Count);
            foreach (var pair in Metadata)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }
    }
}

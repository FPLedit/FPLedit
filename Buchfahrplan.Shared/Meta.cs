﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buchfahrplan.Shared
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

        public TimeSpan GetMetaTimeSpan(string key, TimeSpan defaultValue)
        {
            return GetMeta(key, defaultValue, TimeSpan.Parse);
        }
    }
}

using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FPLedit
{
    internal class Settings : ISettings
    {
        private Configuration config;

        public Settings()
        {
            var path = Assembly.GetEntryAssembly().Location;
            config = ConfigurationManager.OpenExeConfiguration(path);
        }

        public T Get<T>(string key)
            => Get(key, default(T));

        public T Get<T>(string key, T defaultValue)
        {
            if (KeyExists(key))
            {
                var val = config.AppSettings.Settings[key].Value;

                return (T)Convert.ChangeType(val, typeof(T));
            }
            else
                return defaultValue;
        }

        public bool KeyExists(string key)
            => config.AppSettings.Settings.AllKeys.Contains(key);

        public void Remove(string key)
        {
            config.AppSettings.Settings.Remove(key);
            config.Save(ConfigurationSaveMode.Modified, false);
        }

        public void Set(string key, string value)
        {
            if (config.AppSettings.Settings.AllKeys.Contains(key))
                config.AppSettings.Settings[key].Value = value;
            else
                config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Modified, false);
        }

        public void Set(string key, bool value)
            => Set(key, value.ToString());

        public void Set(string key, int value)
            => Set(key, value.ToString());
    }
}

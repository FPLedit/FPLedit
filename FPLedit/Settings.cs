using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using FPLedit.Config;
using System.IO;

namespace FPLedit
{
    internal class Settings : ISettings
    {
        private ConfigFile config;

        public Settings()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            path = Path.Combine(path, "fpledit.conf");

            config = new ConfigFile(TryGetUserPath() ?? path);
        }

        private string TryGetUserPath()
        {
            try
            {
                var appPath = Assembly.GetEntryAssembly().Location;
                var appConfig = ConfigurationManager.OpenExeConfiguration(appPath);
                return appConfig.AppSettings.Settings["config.path"].Value;
            }
            catch
            {
                return null;
            }
        }

        public T Get<T>(string key)
            => Get(key, default(T));

        public T Get<T>(string key, T defaultValue)
        {
            if (KeyExists(key))
            {
                var val = config.Get(key);

                return (T)Convert.ChangeType(val, typeof(T));
            }
            else
                return defaultValue;
        }

        public bool KeyExists(string key)
            => config.KeyExists(key);

        public void Remove(string key)
        {
            config.Remove(key);
            config.Save();
        }

        public void Set(string key, string value)
        {
            config.Set(key, value);
            config.Save();
        }

        public void Set(string key, bool value)
            => Set(key, value.ToString().ToLower());

        public void Set(string key, int value)
            => Set(key, value.ToString());
    }
}

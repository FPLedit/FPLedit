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
        private readonly ConfigFile config;

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

        public T Get<T>(string key, T defaultValue = default)
        {
            var val = config.Get(key);
            if (val != null)
                return (T)Convert.ChangeType(val, typeof(T));
            else
                return defaultValue;
        }

        public T GetEnum<T>(string key, T defaultValue = default) where T : Enum
        {
            var underlying = Enum.GetUnderlyingType(typeof(T));
            var x = (int)Convert.ChangeType(defaultValue, underlying);
            return (T)Enum.ToObject(typeof(T), Get(key, x));
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

        public void SetEnum<T>(string key, T value) where T : Enum
        {
            var underlying = Enum.GetUnderlyingType(typeof(T));
            var x = (int)Convert.ChangeType(value, underlying);
            Set(key, x);
        }
    }
}

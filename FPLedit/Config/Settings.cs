using FPLedit.Shared;
using System;
using System.IO;

namespace FPLedit.Config
{
    internal sealed class Settings : ISettings
    {
        private readonly ConfigFile config;
        private readonly bool isReadonly;

        public Settings()
        {
            var path = Path.Combine(PathManager.Instance.AppDirectory, "fpledit.conf");

            try
            {
                config = new ConfigFile(path);
                var userPath = config.Get("config.path_redirect");
                if (userPath != null && File.Exists(userPath))
                    config = new ConfigFile(userPath);
            }
            catch
            {
                config = new ConfigFile(); // Get in-memory config-backend as we cannot read the specified files (e.g. they do exist but are not readable).
                isReadonly = true;
            }

            try
            {
                config.Save(); // Try to save current config file.
            }
            catch
            {
                isReadonly = true; // We cannot use this file. Thus, we block write access to it.
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
            if (!isReadonly)
                config.Save();
        }

        public void Set(string key, string value)
        {
            config.Set(key, value);
            if (!isReadonly)
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

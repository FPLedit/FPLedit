using FPLedit.Shared;
using System;
using System.IO;

namespace FPLedit.Config
{
    internal sealed class Settings : ISettings, IDisposable
    {
        private ConfigFile config;
        public bool IsReadonly => config.IsReadonly;

        public Settings(Stream stream)
        {
            try
            {
                config = new ConfigFile(stream, false);
            }
            catch
            {
                config?.Dispose(false); // Dispose our last try to get a config file, but not the stream itself.
                stream?.Dispose();
                config = new ConfigFile(); // Get in-memory config-backend as we cannot read the specified files (e.g. they do exist but are not readable).
            }
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            var val = config.Get(key);
            if (val != null)
                return (T)Convert.ChangeType(val, typeof(T));
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
            if (!IsReadonly)
                config.Save();
        }

        public void Set(string key, string value)
        {
            config.Set(key, value);
            if (!IsReadonly)
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

        public void Dispose()
        {
            config?.Dispose();
            config = null;
            GC.SuppressFinalize(this);
        }

        ~Settings()
        {
            Dispose();
        }
    }
}

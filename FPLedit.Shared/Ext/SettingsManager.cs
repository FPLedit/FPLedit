using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace FPLedit.Shared
{
    /// <summary>
    /// Verwaltet die Anwendungseinstellungen.
    /// </summary>
    public static class SettingsManager
    {
        private static Configuration GetConfig()
            => ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);

        /// <summary>
        /// Gibt den Wert einer Einstellung zurück.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        /// <returns>Der Wert der Einstellung oder, falls dieser nicht vorhanden ist, null.</returns>
        public static T Get<T>(string key)
            => Get(key, default(T));

        /// <summary>
        /// Gibt den Wert einer Einstellung zurück.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        /// <param name="defaultValue">Der Standardwert</param>
        /// <returns>Der Wert der Einstellung oder, falls dieser nicht vorhanden ist, der Standardwert.</returns>
        public static T Get<T>(string key, T defaultValue)
        {
            if (KeyExists(key))
            {
                var config = GetConfig();
                var val = config.AppSettings.Settings[key].Value;

                return (T)Convert.ChangeType(val, typeof(T));
            }
            else
                return defaultValue;
        }

        /// <summary>
        /// Überprüft, ob die angegebene Einstellung existeiert.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        /// <returns></returns>
        public static bool KeyExists(string key)
        {
            var config = GetConfig();
            return config.AppSettings.Settings.AllKeys.Contains(key);
        }

        /// <summary>
        /// Setzt den Wert einer Eigenschaft.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        /// <param name="value">Der Wert der Einstellung.</param>
        public static void Set(string key, string value)
        {
            var config = GetConfig();
            if (config.AppSettings.Settings.AllKeys.Contains(key))
                config.AppSettings.Settings[key].Value = value;
            else
                config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Modified, false);
        }

        /// <summary>
        /// Setzt den Wert einer Eigenschaft.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        /// <param name="value">Der Wert der Einstellung.</param>
        public static void Set(string key, bool value)
            => Set(key, value.ToString());

        /// <summary>
        /// Setzt den Wert einer Eigenschaft.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        /// <param name="value">Der Wert der Einstellung.</param>
        public static void Set(string key, int value)
            => Set(key, value.ToString());

        /// <summary>
        /// Entfernt eine Einstellung aus der Konfiguration.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        public static void Remove(string key)
        {
            var config = GetConfig();
            config.AppSettings.Settings.Remove(key);
            config.Save(ConfigurationSaveMode.Modified, false);
        }
    }
}

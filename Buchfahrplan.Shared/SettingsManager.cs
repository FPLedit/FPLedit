using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Buchfahrplan.Shared
{
    /// <summary>
    /// Verwaltet die Anwendungseinstellungen.
    /// </summary>
    public static class SettingsManager
    {
        private static Dictionary<string, string> defaults = new Dictionary<string, string>();
        
        /// <summary>
        /// Gibt den Wert einer Einstellung zurück.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        /// <returns>Der Wert der Einstellung oder, falls dieser nicht vorhanden ist, der Standardwert.</returns>
        public static string Get(string key)
        {
            if (KeyExists(key))
            {
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
                return config.AppSettings.Settings[key].Value;
            }
            else if (defaults.ContainsKey(key))
            {
                return defaults[key];
            }
            else
                return null;
        }

        /// <summary>
        /// Überprüft, ob die angegebene Einstellung existeiert.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        /// <returns></returns>
        public static bool KeyExists(string key)
        {
            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
            return config.AppSettings.Settings.AllKeys.Contains(key);
        }

        /// <summary>
        /// Setzt den Wert einer Eigenschaft.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        /// <param name="value">Der Wert der Einstellung.</param>
        public static void Set(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
            if (config.AppSettings.Settings.AllKeys.Contains(key))
                config.AppSettings.Settings[key].Value = value;
            else
                config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Modified, false);
        }

        /// <summary>
        /// Setzt den Standardwert für eine Einstellung.
        /// </summary>
        /// <param name="key">Der Schlüssel der Einstellung.</param>
        /// <param name="value">Der neue Standardwert.</param>
        public static void SetDefault(string key, string value)
        {
            defaults[key] = value;
        }
    }
}

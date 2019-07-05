using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;

namespace FPLedit
{
    internal class UpdateManager
    {
        public string CheckUrl { get; private set; }

        public Action<bool, VersionInfo> CheckResult { get; set; }

        public Action<string> TextResult { get; set; }

        public Action<Exception> CheckError { get; set; }

        public Version CurrentVersion { get; private set; }

        public bool AutoUpdateEnabled
        {
            get => settings.Get<bool>("updater.auto");
            set => settings.Set("updater.auto", value);
        }

        private readonly ISettings settings;

        public UpdateManager(ISettings settings)
        {
            this.settings = settings;
            CheckUrl = settings.Get("updater.url", "https://fahrplan.manuelhu.de/versioninfo.xml");

            string versionString = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            CurrentVersion = new Version(versionString);
        }

        private VersionInfo GetVersioninfoFromXml(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var ver = doc.DocumentElement.SelectSingleNode("/info/version");
            var url = doc.DocumentElement.SelectSingleNode("/info/url");
            var dsc = doc.DocumentElement.SelectSingleNode("/info/description");
            var txt = doc.DocumentElement.SelectSingleNode("/info/text");

            return new VersionInfo()
            {
                DownloadUrl = url.InnerText,
                NewVersion = new Version(ver.InnerText),
                Description = dsc?.InnerText,
                Text = txt?.InnerText,
            };
        }

        private bool IsNewVersion(Version check) => CurrentVersion.CompareTo(check) < 0;

        public void CheckAsync()
        {
            var url = CheckUrl + "?pf=" + GetPlatform();

            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.DownloadStringAsync(new Uri(url));
                wc.DownloadStringCompleted += (s, e) =>
                {
                    if (e.Error == null && e.Result != "")
                    {
                        try
                        {
                            VersionInfo vi = GetVersioninfoFromXml(e.Result);
                            bool new_avail = IsNewVersion(vi.NewVersion);

                            CheckResult?.Invoke(new_avail, vi);

                            if (vi.Text != null)
                                TextResult?.Invoke(vi.Text);
                        }
                        catch (XmlException ex)
                        {
                            CheckError?.Invoke(ex); // Fehler im XML-Dokument
                        }
                    }
                    else
                        CheckError?.Invoke(e.Error);
                };
            }
        }

        private string GetPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT: return "win";
                case PlatformID.MacOSX: return "macos";
                case PlatformID.Unix: return "unix";
                default: return "other";
            }
        }

        public void AutoUpdateCheck(ILog log)
        {
            // Beispiele für fehlende Funktionen
            if (Environment.OSVersion.Platform != PlatformID.Win32NT && !settings.Get<bool>("mp-compat.disable-startup-warn"))
                log.Warning("Sie verwenden FPLedit nicht auf Windows. Grundsätzlich ist FPLedit zwar mit allen Systemen kompatibel, auf denen Mono läuft, hat aber Einschränkungen in den Funktionen und ist möglicherweise nicht genauso gut getestet.");

            if (settings.Get("updater.auto", "") == "")
            {
                var res = MessageBox.Show("FPLedit kann automatisch bei jedem Programmstart nach einer aktuelleren Version suchen.\n\nDabei werden nur die IP-Adresse und der verwendete Betriebssystemtyp Ihres Computers an den Server übermittelt. Die IP-Adresse wird nur anonymisiert in Log-Dateien gespeichert; ein Rückschluss auf einzelne Benutzer ist daher nicht möglich.", "Automatische Updateprüfung", MessageBoxButtons.YesNo, MessageBoxType.Question);
                settings.Set("updater.auto", res == DialogResult.Yes);
            }

            if (!AutoUpdateEnabled)
                return;

            CheckResult = (new_avail, vi) =>
            {
                if (new_avail)
                    log.Info($"Eine neue Programmversion ({vi.NewVersion.ToString()}) ist verfügbar! {vi.Description ?? ""} Hier herunterladen: {vi.DownloadUrl}");
                else
                    log.Info($"Sie benutzen die aktuelleste Version von FPLedit ({CurrentVersion.ToString()})!");
            };

            TextResult = t => log.Info(t);

            CheckAsync();
        }

        public class VersionInfo
        {
            public string DownloadUrl;
            public Version NewVersion;
            public string Description;
            public string Text;
        }
    }
}

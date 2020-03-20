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
    internal sealed class UpdateManager
    {
        public string CheckUrl { get; }

        public Action<bool, VersionInfo> CheckResult { get; set; }

        public Action<string> TextResult { get; set; }

        public Action<Exception> CheckError { get; set; }

        public Version CurrentVersion { get; }
        
        public string CurrentVersionDisplay { get; }

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

            string versionString = FileVersionInfo.GetVersionInfo(PathManager.Instance.AppFilePath).ProductVersion;
            CurrentVersion = new Version(versionString);

            var attr = typeof(MainForm).Assembly.GetCustomAttribute<AssemblyVersionFlagAttribute>();
            CurrentVersionDisplay = CurrentVersion + (attr != null ? "-" + attr.Flag : "");
        }

        private VersionInfo GetVersioninfoFromXml(string xml)
        {
            var doc = new XmlDocument { XmlResolver = null };

            var xmlReaderSettings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
            };

            using (var reader = XmlReader.Create(new System.IO.StringReader(xml), xmlReaderSettings))
                doc.Load(reader);

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
                    log.Info($"Eine neue Programmversion ({vi.NewVersion}) ist verfügbar! {vi.Description ?? ""} Hier herunterladen: {vi.DownloadUrl}");
                else
                    log.Info($"Sie benutzen die aktuelleste Version von FPLedit ({CurrentVersionDisplay})!");
            };

            TextResult = log.Info;

            CheckAsync();
        }

        public struct VersionInfo
        {
            public string DownloadUrl;
            public Version NewVersion;
            public string Description;
            public string Text;
        }
    }
}

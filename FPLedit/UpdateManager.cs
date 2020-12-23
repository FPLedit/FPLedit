using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Net;
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

        private Version CurrentVersion { get; }
        
        private string CurrentVersionDisplay { get; }

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

            CurrentVersion = VersionInformation.Current.AppBaseVersion;
            CurrentVersionDisplay = VersionInformation.Current.DisplayVersion;
        }

        private VersionInfo GetUpdateInfoFromXml(string xml)
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

        private bool IsNewVersion(Version check) => CurrentVersion.CompareTo(check) < 0 || (CurrentVersion.CompareTo(check) == 0 && !string.IsNullOrEmpty(VersionInformation.Current.VersionSuffix));

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
                            VersionInfo vi = GetUpdateInfoFromXml(e.Result);
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
                var res = MessageBox.Show(T._("FPLedit kann automatisch bei jedem Programmstart nach einer aktuelleren Version suchen.\n\nDabei werden nur die IP-Adresse und der verwendete Betriebssystemtyp Ihres Computers an den Server übermittelt. Die IP-Adresse wird nur anonymisiert in Log-Dateien gespeichert; ein Rückschluss auf einzelne Benutzer ist daher nicht möglich."), T._("Automatische Updateprüfung"), MessageBoxButtons.YesNo, MessageBoxType.Question);
                settings.Set("updater.auto", res == DialogResult.Yes);
            }

            if (!AutoUpdateEnabled)
                return;

            CheckResult = (newAvailable, vi) =>
            {
                if (newAvailable)
                    log.Info(T._("Eine neue Programmversion ({0}) ist verfügbar! {1} Hier herunterladen: {2}", vi.NewVersion, vi.Description ?? "", vi.DownloadUrl));
                else
                    log.Info(T._("Sie benutzen die aktuelleste Version von FPLedit ({0})!", CurrentVersionDisplay));
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

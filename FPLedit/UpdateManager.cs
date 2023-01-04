#nullable enable
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

        public Action<bool, VersionInfo>? CheckResult { get; set; }

        public Action<string>? TextResult { get; set; }

        public Action<Exception?>? CheckError { get; set; }

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

        private VersionInfo? GetUpdateInfoFromXml(string xml)
        {
            var doc = new XmlDocument { XmlResolver = null! };

            var xmlReaderSettings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
            };

            using (var reader = XmlReader.Create(new System.IO.StringReader(xml), xmlReaderSettings))
                doc.Load(reader);
            
            if (doc.DocumentElement == null) return null;

            var ver = doc.DocumentElement.SelectSingleNode("/info/version");
            var url = doc.DocumentElement.SelectSingleNode("/info/url");
            var dsc = doc.DocumentElement.SelectSingleNode("/info/description");
            var txt = doc.DocumentElement.SelectSingleNode("/info/text");

            if (url == null || ver == null) return null;

            return new VersionInfo(url.InnerText, new Version(ver.InnerText), dsc?.InnerText, txt?.InnerText);
        }

        private bool IsNewVersion(Version check) 
            => CurrentVersion.CompareTo(check) < 0 || 
               (CurrentVersion.CompareTo(check) == 0 && (!string.IsNullOrEmpty(VersionInformation.Current.VersionSuffix) || VersionInformation.Current.IsDevelopmentVersion));

        public void CheckAsync()
        {
            var url = CheckUrl + "?pf=" + GetPlatform();

            using var wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.DownloadStringAsync(new Uri(url));
            wc.DownloadStringCompleted += (_, e) =>
            {
                if (e.Error == null && e.Result != "")
                {
                    try
                    {
                        var vi = GetUpdateInfoFromXml(e.Result);
                        if (vi != null)
                        {
                            var hasNewVersion = IsNewVersion(vi.NewVersion);
                            CheckResult?.Invoke(hasNewVersion, vi);

                            if (vi.Text != null)
                                TextResult?.Invoke(vi.Text);
                        }
                        else
                            CheckError?.Invoke(null);
                    }
                    catch (Exception ex)
                    {
                        CheckError?.Invoke(ex); // Error in the XML document.
                    }
                }
                else
                    CheckError?.Invoke(e.Error);
            };
        }

        private string GetPlatform()
        {
            return Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "win",
                PlatformID.MacOSX => "macos",
                PlatformID.Unix => "unix",
                _ => "other"
            };
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

        public record VersionInfo(string DownloadUrl, Version NewVersion, string? Description, string? Text);
    }
}

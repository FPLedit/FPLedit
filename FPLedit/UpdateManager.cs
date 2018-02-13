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

        public Action<VersionInfo> CheckResult { get; set; }

        public Action<string> TextResult { get; set; }

        public Action<Exception> CheckError { get; set; }

        public bool AutoUpdateEnabled
        {
            get => settings.Get<bool>("updater.auto");
            set => settings.Set("updater.auto", value);
        }

        private ISettings settings;

        public UpdateManager(ISettings settings)
        {
            this.settings = settings;
            CheckUrl = settings.Get("updater.url", "https://fahrplan.manuelhu.de/versioninfo.xml");
        }

        public Version GetCurrentVersion()
        {
            string versionString = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            return new Version(versionString);
        }

        public VersionInfo GetVersioninfoFromXml(string xml)
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

        public bool IsNewVersion(Version check)
        {
            return GetCurrentVersion().CompareTo(check) < 0;
        }

        public void CheckAsync()
        {
            string platform = "other";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                platform = "mswin";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major < 6)
                platform = "mswinxp";
            if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                platform = "macos";
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                platform = "unix";
            var url = CheckUrl + "?pf=" + platform;

            using (WebClient wc = new WebClient())
            {
                wc.DownloadStringAsync(new Uri(url));
                wc.DownloadStringCompleted += (s, e) =>
                {
                    if (e.Error == null && e.Result != "")
                    {
                        try
                        {
                            VersionInfo info = GetVersioninfoFromXml(e.Result);
                            bool newAvailable = IsNewVersion(info.NewVersion);

                            if (newAvailable)
                                CheckResult?.Invoke(info);
                            else
                                CheckResult?.Invoke(null);

                            if (info.Text != null)
                                TextResult?.Invoke(info.Text);
                        }
                        catch (XmlException ex)
                        {
                        // Fehler im XML-Dokument
                        CheckError?.Invoke(ex);
                        }
                    }
                    else
                        CheckError?.Invoke(e.Error);
                };
            }
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

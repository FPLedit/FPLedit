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

        public Action<Exception> CheckError { get; set; }

        public UpdateManager()
        {
            CheckUrl = SettingsManager.Get("updater.url");
        }

        public Version GetCurrentVersion()
        {
            string versionString = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            return new Version(versionString);
        }

        public VersionInfo GetVersioninfoFromXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNode ver = doc.DocumentElement.SelectSingleNode("/info/version");
            XmlNode url = doc.DocumentElement.SelectSingleNode("/info/url");
            XmlNode dsc = doc.DocumentElement.SelectSingleNode("/info/description");

            return new VersionInfo()
            {
                DownloadUrl = url.InnerText,
                NewVersion = new Version(ver.InnerText),
                Description = dsc?.InnerText,
            };
        }

        public bool IsNewVersion(Version check)
        {
            return GetCurrentVersion().CompareTo(check) < 0;
        }

        public void CheckAsync()
        {
            WebClient wc = new WebClient();
            wc.DownloadStringAsync(new Uri(CheckUrl));
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error == null && e.Result != "")
                {
                    VersionInfo info = GetVersioninfoFromXml(e.Result);
                    bool newAvailable = IsNewVersion(info.NewVersion);

                    if (newAvailable)
                        CheckResult?.Invoke(info);
                    else
                        CheckResult?.Invoke(null);
                }
                else
                    CheckError?.Invoke(e.Error);
            };
        }

        public class VersionInfo
        {
            public string DownloadUrl;
            public Version NewVersion;
            public string Description;
        }
    }
}

using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace FPLedit
{
    public partial class InfoForm : Form
    {
        public InfoForm()
        {
            InitializeComponent();

            licenseLabel.Text = Properties.Resources.Info;

            versionLabel.Text = versionLabel.Text.Replace("{version}", GetVersion());
        }        

        private string GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
        }

        private void VersionCheck()
        {
            checkButton.Enabled = false;
            WebClient wc = new WebClient();
            wc.DownloadStringAsync(new Uri(string.Format(SettingsManager.Get("CheckUrl"), GetVersion())));
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error == null && e.Result != "")
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(e.Result);

                    XmlNode ver = doc.DocumentElement.SelectSingleNode("/info/version");
                    XmlNode url = doc.DocumentElement.SelectSingleNode("/info/url");

                    Version appVersion = new Version(GetVersion());
                    Version onlineVersion = new Version(ver.InnerText);

                    bool newAvailable = appVersion.CompareTo(onlineVersion) < 0;

                    if (newAvailable)
                    {
                        string nl = Environment.NewLine;
                        DialogResult res = MessageBox.Show($"Eine neue Programmversion ({ver.InnerText}) ist verfügbar!{nl}{nl}Jetzt zur Download-Seite wechseln, um die neue Version herunterzuladen?",
                            "Neue FPLedit-Version verfügbar", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                        if (res == DialogResult.Yes)
                            Process.Start(url.InnerText);
                    }
                    else
                    {
                        MessageBox.Show($"Sie benutzen bereits die aktuelle Version!",
                        "Auf neue Version prüfen");
                    }
                }
                else
                {
                    MessageBox.Show($"Verbindung mit dem Server fehlgeschlagen!",
                        "Auf neue Version prüfen");
                }
            };
        }

        private void websiteLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/");

        private void checkButton_Click(object sender, EventArgs e)
            => VersionCheck();
    }
}

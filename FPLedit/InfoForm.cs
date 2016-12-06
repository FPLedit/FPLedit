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
            UpdateManager mg = new UpdateManager();

            licenseLabel.Text = Properties.Resources.Info;

            versionLabel.Text = versionLabel.Text.Replace("{version}", mg.GetCurrentVersion().ToString());
        }

        private void VersionCheck()
        {
            UpdateManager mg = new UpdateManager();
            mg.CheckResult = vi =>
            {
                if (vi != null)
                {
                    string nl = Environment.NewLine;
                    DialogResult res = MessageBox.Show($"Eine neue Programmversion ({vi.Version.ToString()}) ist verfügbar!{nl}{nl}Jetzt zur Download-Seite wechseln, um die neue Version herunterzuladen?",
                        "Neue FPLedit-Version verfügbar", MessageBoxButtons.YesNo);

                    if (res == DialogResult.Yes)
                        Process.Start(vi.DownloadUrl);
                }
                else
                {
                    MessageBox.Show($"Sie benutzen bereits die aktuelle Version!",
                        "Auf neue Version prüfen");
                }
            };
            mg.CheckError = ex =>
            {
                MessageBox.Show($"Verbindung mit dem Server fehlgeschlagen!",
                    "Auf neue Version prüfen");
            };

            mg.CheckAsync();
            checkButton.Enabled = false;
        }

        private void websiteLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/");

        private void checkButton_Click(object sender, EventArgs e)
            => VersionCheck();
    }
}

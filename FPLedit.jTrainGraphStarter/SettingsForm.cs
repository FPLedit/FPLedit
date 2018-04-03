using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.jTrainGraphStarter
{
    public partial class SettingsForm : Form
    {
        private ISettings settings;

        private SettingsForm()
        {
            InitializeComponent();
        }

        public SettingsForm(ISettings settings) : this()
        {
            this.settings = settings;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            var versions = new[]
            {
                new VersionItem(TimetableVersion.JTG2_x, "2.0x"),
                new VersionItem(TimetableVersion.JTG3_0, "3.0x"),
            };
            versionComboBox.Items.AddRange(versions);

            javaPathTextBox.Text = settings.Get("jTGStarter.javapath", "");
            jtgPathTextBox.Text = settings.Get("jTGStarter.jtgpath", "jTrainGraph_301.jar");
            messageCheckBox.Checked = !settings.Get("jTGStarter.show-message", true);

            var targetVersion = (TimetableVersion)settings.Get("jTGStarter.target-version", 008);
            var vidx = Array.FindIndex(versions, v => v.Version == targetVersion);
            versionComboBox.SelectedIndex = vidx == -1 ? 0 : vidx;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            bool jtgexists = File.Exists(jtgPathTextBox.Text) || ExecutableExists(jtgPathTextBox.Text);
            bool javaexists = ExecutableExists(javaPathTextBox.Text);

            if (!javaexists || !jtgexists)
            {
                var text = "";
                if (!jtgexists)
                    text += "Die angegebene Datei für jTrainGraph wurde nicht gefunden. ";
                if (!javaexists)
                    text += "Java wurde unter dem angegebenen Pfad nicht gefunden. ";
                text += "Wollen Sie trotzdem fortfahren?";
                if (MessageBox.Show(text, "jTrainGraphStarter: Fehler", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            var targetVersion = (int)((VersionItem)versionComboBox.SelectedItem).Version;

            settings.Set("jTGStarter.target-version", targetVersion);
            settings.Set("jTGStarter.show-message", !messageCheckBox.Checked);
            settings.Set("jTGStarter.javapath", javaPathTextBox.Text);
            settings.Set("jTGStarter.jtgpath", jtgPathTextBox.Text);
            Close();
        }

        private bool ExecutableExists(string path)
        {
            bool exists = true;
            try
            {
                var p = Process.Start(path);
                p.Kill();
            }
            catch { exists = false; }

            return exists;
        }

        private void docLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/bildfahrplaene/");

        private void downloadLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start("http://kinzigtalbahn.bplaced.net/homepage/programme.html");

        private class VersionItem
        {
            public TimetableVersion Version { get; set; }

            public string Name { get; set; }

            public VersionItem(TimetableVersion version, string name)
            {
                Version = version;
                Name = name;
            }

            public override string ToString()
                => Name;
        }
    }
}

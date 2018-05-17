using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FPLedit.Shared.UI;

namespace FPLedit.jTrainGraphStarter
{
    internal class SettingsForm : Dialog<DialogResult>
    {
        private ISettings settings;

#pragma warning disable CS0649
        private TextBox javaPathTextBox, jtgPathTextBox;
        private DropDown versionComboBox;
        private CheckBox messageCheckBox;
#pragma warning restore CS0649

        public SettingsForm(ISettings settings)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.settings = settings;

            var versions = new[]
            {
                new VersionItem(TimetableVersion.JTG2_x, "2.0x"),
                new VersionItem(TimetableVersion.JTG3_0, "3.0x"),
            };
            versionComboBox.DataStore = versions;
            versionComboBox.ItemTextBinding = Binding.Property<VersionItem, string>(vi => vi.Name);

            javaPathTextBox.Text = settings.Get("jTGStarter.javapath", "");
            jtgPathTextBox.Text = settings.Get("jTGStarter.jtgpath", "jTrainGraph_302.jar");
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

            var targetVersion = (int)((VersionItem)versionComboBox.SelectedValue).Version;

            settings.Set("jTGStarter.target-version", targetVersion);
            settings.Set("jTGStarter.show-message", !messageCheckBox.Checked.Value);
            settings.Set("jTGStarter.javapath", javaPathTextBox.Text);
            settings.Set("jTGStarter.jtgpath", jtgPathTextBox.Text);
            Close();
        }

        private void chooseJtgButton_Click(object sender, EventArgs e)
        {
            var sfd = new OpenFileDialog();
            sfd.AddLegacyFilter("JAR-Dateien (*.jar)|*.jar");
            if (sfd.ShowDialog(this) == DialogResult.Ok)
                jtgPathTextBox.Text = sfd.FileName;
        }

        private void findJavaButton_Click(object sender, EventArgs e)
            => javaPathTextBox.Text = JavaFinder.JavaGuess() ?? javaPathTextBox.Text;

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

        private void docLinkLabel_LinkClicked(object sender, EventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/bildfahrplaene/");

        private void downloadLinkLabel_LinkClicked(object sender, EventArgs e)
            => Process.Start("https://jtraingraph.de");

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

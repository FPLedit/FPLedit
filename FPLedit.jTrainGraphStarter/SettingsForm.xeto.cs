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
    internal sealed class SettingsForm : FDialog<DialogResult>
    {
        private readonly ISettings settings;

#pragma warning disable CS0649
        private readonly TextBox javaPathTextBox, jtgPathTextBox;
        private readonly DropDown versionComboBox;
        private readonly CheckBox messageCheckBox;
#pragma warning restore CS0649

        public SettingsForm(ISettings settings)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.settings = settings;

            var versions = new[]
            {
                new VersionItem(TimetableVersion.JTG2_x, "2.0x"),
                new VersionItem(TimetableVersion.JTG3_0, "3.0x"),
                new VersionItem(TimetableVersion.JTG3_1, "3.1x"),
            };
            versionComboBox.DataStore = versions;
            versionComboBox.ItemTextBinding = Binding.Property<VersionItem, string>(vi => vi.Name);

            javaPathTextBox.Text = settings.Get("jTGStarter.javapath", "");
            jtgPathTextBox.Text = settings.Get("jTGStarter.jtgpath", JtgShared.DEFAULT_FILENAME);
            messageCheckBox.Checked = !settings.Get("jTGStarter.show-message", true);

            var targetVersion = settings.GetEnum("jTGStarter.target-version", JtgShared.DEFAULT_TT_VERSION);
            var vidx = Array.FindIndex(versions, v => v.Version == targetVersion);
            versionComboBox.SelectedIndex = vidx == -1 ? 0 : vidx;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            bool jtgexists = File.Exists(jtgPathTextBox.Text) || ExecutableExists(jtgPathTextBox.Text);
            bool javaexists = ExecutableExists(javaPathTextBox.Text);
            var compat = JtgShared.JtgCompatCheck(jtgPathTextBox.Text);

            if (!javaexists || !jtgexists || !compat.Compatible)
            {
                var text = "";
                if (!jtgexists)
                    text += "Die angegebene Datei für jTrainGraph wurde nicht gefunden. ";
                if (!compat.Compatible)
                    text += "Die gewählte Version von jTrainGraph ist wahrscheinlich nicht mit FPledit kompatibel. Bitte verwenden Sie jTrainGraph 2.02 - 2.03 oder 3.03 (und höher)! ";
                if (!javaexists)
                    text += "Java wurde unter dem angegebenen Pfad nicht gefunden. ";
                text += "Wollen Sie trotzdem fortfahren?";
                if (MessageBox.Show(text, "jTrainGraphStarter: Fehler", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            var targetVersion = ((VersionItem)versionComboBox.SelectedValue).Version;

            settings.SetEnum("jTGStarter.target-version", targetVersion);
            settings.Set("jTGStarter.show-message", !messageCheckBox.Checked.Value);
            settings.Set("jTGStarter.javapath", javaPathTextBox.Text);
            settings.Set("jTGStarter.jtgpath", jtgPathTextBox.Text);
            Close();
        }

        private void ChooseJtgButton_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.AddLegacyFilter("JAR-Dateien (*.jar)|*.jar");
                if (ofd.ShowDialog(this) == DialogResult.Ok)
                {
                    jtgPathTextBox.Text = ofd.FileName;

                    var compat = JtgShared.JtgCompatCheck(jtgPathTextBox.Text);
                    if (compat.Version.HasValue)
                        versionComboBox.SelectedValue = versionComboBox.DataStore.
                            FirstOrDefault(i => ((VersionItem)i).Version == compat.Version);
                }
            }
        }

        private void FindJavaButton_Click(object sender, EventArgs e)
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

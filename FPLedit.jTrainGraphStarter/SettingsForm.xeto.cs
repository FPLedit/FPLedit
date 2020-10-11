using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                new VersionItem(TimetableVersion.JTG3_1, "3.1x"),
                new VersionItem(TimetableVersion.JTG3_2, "3.2x"),
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
                    text += T._("Die angegebene Datei für jTrainGraph wurde nicht gefunden. ");
                if (!compat.Compatible)
                    text += T._("Die gewählte Version von jTrainGraph ist wahrscheinlich nicht mit FPledit kompatibel. Bitte verwenden Sie jTrainGraph in einer kompatiblen Version! ");
                if (!javaexists)
                    text += T._("Java wurde unter dem angegebenen Pfad nicht gefunden. ");
                text += T._("Wollen Sie trotzdem fortfahren?");
                if (MessageBox.Show(text, T._("jTrainGraphStarter: Fehler"), MessageBoxButtons.YesNo) == DialogResult.No)
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
                ofd.Title = T._("jTrainGraph-Programmdatei wählen");
                ofd.AddLegacyFilter(T._("JAR-Dateien (*.jar)|*.jar"));
                
                if (!string.IsNullOrWhiteSpace(jtgPathTextBox.Text))
                {
                    var directory = Path.GetDirectoryName(jtgPathTextBox.Text);
                    if (Directory.Exists(directory))
                        ofd.Directory = new Uri(directory);
                }

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

        private static class L
        {
            public static readonly string Title = T._("jTrainGraphStarter Einstellungen");
            public static readonly string Close = T._("Schließen");
            
            public static readonly string JavaPathLabel = T._("Java-Pfad");
            public static readonly string FindJava = T._("Erkennen");
            public static readonly string JavaPathDescription = T._("i.d.R: Windows: javaw.exe, Linux: java");
            
            public static readonly string JtgPathLabel = T._("Pfad zur jTrainGraph-Datei");
            public static readonly string JtgPathDescription = T._("Anwendungsdatei von jTrainGraph (normalerweise jTrainGraph_xxx.jar/exe, wobei xxx eine kompatible Version von jTrainGraph ist)");
            public static readonly string JtgVersionLabel = T._("Version von jTrainGraph");
            
            public static readonly string DocsLabel = T._("Weitere Hinweise und kompatible Versionen:");
            public static readonly string DocsLink = T._("https://fahrplan.manuelhu.de/bildfahrplaene/");
            public static readonly string DocsLinkText = T._("Dokumentation zu diesem Plugin");
            
            public static readonly string DownloadLabel = T._("Wenn Sie jTrainGraph noch gar nicht installiert haben:");
            public static readonly string DownloadLink = T._("https://jtraingraph.de");
            public static readonly string DownloadLinkText = T._("jTrainGraph herunterladen");
            public static readonly string FilenameNote = T._("Auch wenn jTrainGraph in der *.exe-Variante verwendet wird bitte den Java-Pfad ausfüllen!");
            public static readonly string CurrentNote = T._("Bitte verwenden Sie, wenn möglich, immer die aktuelleste jTrainGraph-Version!");
            public static readonly string CurrentList = T._("Kompatible Versionen sind 2.02, 2.03, 3.03, 3.11");
            
            public static readonly string MessageLabel = T._("Warnhinweis nicht bei jedem jTrainGraph-Start anzeigen");
            
            
        }
    }
}

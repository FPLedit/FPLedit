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

#pragma warning disable CS0649,CA2213
        private readonly TextBox javaPathTextBox = default!, jtgPathTextBox = default!;
        private readonly DropDown versionComboBox = default!;
        private readonly CheckBox messageCheckBox = default!;
#pragma warning restore CS0649,CA2213

        public SettingsForm(ISettings settings)
        {
            // Load versions before loading UI xaml.
            var versions = TimetableVersionExt.GetAllVersionInfos()
                .Where(c => c.Compatibility == TtVersionCompatType.ReadWrite)
                .Where(c => c.JtgVersionCompatibility.Any())
                .SelectMany(c => 
                    c.JtgVersionCompatibility.Select(j => new VersionItem(c.Version, j.version.Replace("*", "x"))))
                .ToArray();

            L.CompatibleVersions = string.Join(", ", versions.Select(vi => vi.Name));
            
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.settings = settings;

            versionComboBox.DataStore = versions;
            versionComboBox.ItemTextBinding = Binding.Delegate<VersionItem, string>(vi => vi.Name);

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
            var compat = JtgShared.JtgCompatCheck(jtgPathTextBox.Text, out _);

            if (!javaexists || !jtgexists || !compat)
            {
                var text = "";
                if (!jtgexists)
                    text += T._("Die angegebene Datei für jTrainGraph wurde nicht gefunden. ");
                if (!compat)
                    text += T._("Die gewählte Version von jTrainGraph ist wahrscheinlich nicht mit FPledit kompatibel. Bitte verwenden Sie jTrainGraph in einer kompatiblen Version! ");
                if (!javaexists)
                    text += T._("Java wurde unter dem angegebenen Pfad nicht gefunden. ");
                text += T._("Wollen Sie trotzdem fortfahren?");
                if (MessageBox.Show(text, T._("jTrainGraphStarter: Fehler"), MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            var targetVersion = ((VersionItem)versionComboBox.SelectedValue).Version;

            settings.SetEnum("jTGStarter.target-version", targetVersion);
            settings.Set("jTGStarter.show-message", !messageCheckBox.Checked!.Value);
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

                    JtgShared.JtgCompatCheck(jtgPathTextBox.Text, out var compatVersion);
                    if (compatVersion.HasValue)
                        versionComboBox.SelectedValue = versionComboBox.DataStore.
                            FirstOrDefault(i => ((VersionItem)i).Version == compatVersion);
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
                p!.Kill();
            }
            catch { exists = false; }

            return exists;
        }

        private record VersionItem(TimetableVersion Version, string Name)
        {
            public override string ToString() => Name;
        }

        private static class L
        {
            public static string CompatibleVersions { get; set; }
            
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
            public static string CurrentList => T._("Kompatible Versionen sind {0}", CompatibleVersions);
            
            public static readonly string MessageLabel = T._("Warnhinweis nicht bei jedem jTrainGraph-Start anzeigen");
        }
    }
}

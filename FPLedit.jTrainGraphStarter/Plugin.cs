using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.jTrainGraphStarter
{
    [Plugin("Starter für jTrainGraph", Pvi.From, Pvi.UpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        IInfo info;
        ButtonMenuItem startItem, settingsItem;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            var item = ((MenuBar)info.Menu).CreateItem("jTrainGraph");

            startItem = item.CreateItem("jTrainGraph starten");
            startItem.Enabled = false;
            startItem.Click += (s, e) =>
            {
                if (info.Timetable.Type == TimetableType.Linear)
                    StartLinear();
                else
                    StartNetwork(info.FileState.SelectedRoute);
            };

            settingsItem = item.CreateItem("Einstellungen");
            settingsItem.Click += (s, e) => (new SettingsForm(info.Settings)).ShowModal(info.RootForm);
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            startItem.Enabled = e.FileState.Opened;

            startItem.Text = (e.FileState.Opened && info.Timetable.Type == TimetableType.Network) ?
                "jTrainGraph starten (aktuelle Route)" : "jTrainGraph starten";
        }

        private void StartLinear()
        {
            bool showMessage = info.Settings.Get("jTGStarter.show-message", true);

            if (showMessage)
            {
                DialogResult res = MessageBox.Show("Dies speichert die Fahrplandatei am letzten Speicherort und öffnet dann jTrainGraph (>= 2.02). Nachdem Sie die Arbeit in jTrainGraph beendet haben, speichern Sie damit die Datei und schließen das jTrainGraph-Hauptfenster, damit werden die Änderungen übernommen. Aktion fortsetzen?" + Environment.NewLine + Environment.NewLine + "Diese Meldung kann unter jTrainGraph > Einstellungen deaktiviert werden.",
                    "jTrainGraph starten", MessageBoxButtons.YesNo, MessageBoxType.Warning);

                if (res != DialogResult.Yes)
                    return;
            }

            info.Save(false);

            StartJtg(info.FileState.FileName, () => info.Reload());
        }

        private void StartNetwork(int route)
        {
            bool showMessage = info.Settings.Get("jTGStarter.show-message", true);

            if (showMessage)
            {
                DialogResult res = MessageBox.Show("Dies speichert die aktuell ausgewählte Route in eine temporäre Datei und öffnet dann jTrainGraph (>= 2.02). Nachdem Sie die Arbeit in jTrainGraph beendet haben, speichern Sie damit die Datei und schließen das jTrainGraph-Hauptfenster, damit werden alle Änderungen an den Bildfahrplaneinstellungen übernommen."
                    + Environment.NewLine + "ACHTUNG: Es werden nur Änderungen an der Bildfahrplandarstellung übernommen, alle anderen Änderungen (z.B. Bahnhöfe oder Züge einfügen) werden verworfen! Aktion fortsetzen?"
                    + Environment.NewLine + Environment.NewLine + "Diese Meldung kann unter jTrainGraph > Einstellungen deaktiviert werden.",
                    "jTrainGraph starten (aktuelle Route)", MessageBoxButtons.YesNo, MessageBoxType.Warning);

                if (res != DialogResult.Yes)
                    return;
            }

            int targetVersion = info.Settings.Get("jTGStarter.target-version", JTGShared.DEFAULT_TT_VERSION);

            var exporter = new Shared.Filetypes.XMLExport();
            var importer = new Shared.Filetypes.XMLImport();
            var sync = new TimetableRouteSync(info.Timetable, route);
            var rtt = sync.GetRouteTimetable((TimetableVersion)targetVersion);
            var fn = info.GetTemp("route-" + route + ".fpl");
            exporter.Export(rtt, fn, info);

            StartJtg(fn, () =>
            {
                info.StageUndoStep();
                var crtt = importer.Import(fn, new SilentLogger());
                sync.SyncBack(crtt);
                info.SetUnsaved();
            });
        }

        private void StartJtg(string fnArg, Action finished)
        {
            string javapath = info.Settings.Get("jTGStarter.javapath", "java");
            string jtgPath = info.Settings.Get("jTGStarter.jtgpath", JTGShared.DEFAULT_FILENAME);

            var compat = JTGShared.JTGCompatCheck(jtgPath);
            if (!compat.Compatible)
            {
                MessageBox.Show("Die gewählte Version von jTrainGraph ist wahrscheinlich nicht mit FPledit kompatibel. Bitte verwenden Sie jTrainGraph 2.02 - 2.03 oder 3.03 (und höher)!",
                    "jTrainGraphStarter: Fehler", MessageBoxType.Error);
                return;
            }

            string jtgFolder = Path.GetDirectoryName(jtgPath);

            Process p = new Process();
            p.StartInfo.WorkingDirectory = jtgFolder;
            p.StartInfo.FileName = javapath;
            p.StartInfo.Arguments = "-jar \"" + jtgPath + "\" \"" + fnArg + "\"";

            try
            {
                p.Start();
                info.Logger.Info("Wartet darauf, dass jTrainGraph beendet wird...");
                p.WaitForExit();
                info.Logger.Info("jTrainGraph beendet! Lade Datei neu...");

                finished();
            }
            catch (Exception e)
            {
                info.Logger.Error("jTrainGraphStarter: " + e.Message);
                info.Logger.Error("Möglicherweise ist das jTrainGraphStarter Plugin falsch konfiguriert! Zur Konfiguration siehe jTrainGraph > Einstellungen");
            }
        }
    }
}

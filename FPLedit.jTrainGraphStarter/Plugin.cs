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
    [Plugin("Starter für jTrainGraph", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        IPluginInterface pluginInterface;
        ButtonMenuItem startItem;

        public void Init(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;

            var item = ((MenuBar)pluginInterface.Menu).CreateItem("jTrainGraph");

            startItem = item.CreateItem("jTrainGraph starten", enabled: false);
            startItem.Click += (s, e) =>
            {
                if (pluginInterface.Timetable.Type == TimetableType.Linear)
                    StartLinear();
                else
                    StartNetwork(pluginInterface.FileState.SelectedRoute);
            };

            item.CreateItem("Einstellungen", clickHandler: (s, e) => (new SettingsForm(pluginInterface.Settings)).ShowModal(pluginInterface.RootForm));
        }

        private void PluginInterface_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            startItem.Enabled = e.FileState.Opened;

            startItem.Text = (e.FileState.Opened && pluginInterface.Timetable.Type == TimetableType.Network) ?
                "jTrainGraph starten (aktuelle Route)" : "jTrainGraph starten";
        }

        private void StartLinear()
        {
            var backupHandle = pluginInterface.BackupTimetable();
            try
            {
                bool showMessage = pluginInterface.Settings.Get("jTGStarter.show-message", true);

                if (showMessage)
                {
                    DialogResult res = MessageBox.Show("Dies speichert die Fahrplandatei am letzten Speicherort und öffnet dann jTrainGraph (>= 2.02). Nachdem Sie die Arbeit in jTrainGraph beendet haben, speichern Sie damit die Datei und schließen das jTrainGraph-Hauptfenster, damit werden die Änderungen übernommen. Aktion fortsetzen?" + Environment.NewLine + Environment.NewLine + "Diese Meldung kann unter jTrainGraph > Einstellungen deaktiviert werden.",
                        "jTrainGraph starten", MessageBoxButtons.YesNo, MessageBoxType.Warning);

                    if (res != DialogResult.Yes)
                        return;
                }

                pluginInterface.Save(false);

                StartJtg(pluginInterface.FileState.FileName, () => pluginInterface.Reload());
                pluginInterface.ClearBackup(backupHandle);
            }
            catch (Exception e)
            {
                pluginInterface.Logger.Error("Beim Verwenden von jTrainGraph ist ein unerwarteter Fehler aufgetreten! " + e);
                pluginInterface.Logger.LogException(e);
                pluginInterface.RestoreTimetable(backupHandle);
            }
        }

        private void StartNetwork(int route)
        {
            var backupHandle = pluginInterface.BackupTimetable();
            try
            {
                bool showMessage = pluginInterface.Settings.Get("jTGStarter.show-message", true);

                if (showMessage)
                {
                    DialogResult res = MessageBox.Show("Dies speichert die aktuell ausgewählte Route in eine temporäre Datei und öffnet dann jTrainGraph (>= 2.02). Nachdem Sie die Arbeit in jTrainGraph beendet haben, speichern Sie damit die Datei und schließen das jTrainGraph-Hauptfenster, damit werden alle Änderungen an den Bildfahrplaneinstellungen übernommen."
                        + Environment.NewLine + "ACHTUNG: Es werden nur Änderungen an der Bildfahrplandarstellung übernommen, alle anderen Änderungen (z.B. Bahnhöfe oder Züge einfügen) werden verworfen! Aktion fortsetzen?"
                        + Environment.NewLine + Environment.NewLine + "Diese Meldung kann unter jTrainGraph > Einstellungen deaktiviert werden.",
                        "jTrainGraph starten (aktuelle Route)", MessageBoxButtons.YesNo, MessageBoxType.Warning);

                    if (res != DialogResult.Yes)
                        return;
                }

                var targetVersion = pluginInterface.Settings.GetEnum("jTGStarter.target-version", JTGShared.DEFAULT_TT_VERSION);

                var exporter = new Shared.Filetypes.XMLExport();
                var importer = new Shared.Filetypes.XMLImport();
                var sync = new TimetableRouteSync(pluginInterface.Timetable, route);
                var rtt = sync.GetRouteTimetable(targetVersion);
                var fn = pluginInterface.GetTemp(Guid.NewGuid().ToString() + "-route-" + route + ".fpl");
                exporter.Export(rtt, fn, pluginInterface);

                StartJtg(fn, () =>
                {
                    pluginInterface.StageUndoStep();
                    var crtt = importer.Import(fn, pluginInterface, new SilentLogger(pluginInterface.Logger));
                    sync.SyncBack(crtt);
                    pluginInterface.SetUnsaved();
                });
                pluginInterface.ClearBackup(backupHandle);
            }
            catch (Exception e)
            {
                pluginInterface.Logger.Error("Beim Verwenden von jTrainGraph ist ein unerwarteter Fehler aufgetreten! " + e);
                pluginInterface.Logger.LogException(e);
                pluginInterface.RestoreTimetable(backupHandle);
            }
        }

        private void StartJtg(string fnArg, Action finished)
        {
            string javapath = pluginInterface.Settings.Get("jTGStarter.javapath", "java");
            string jtgPath = pluginInterface.Settings.Get("jTGStarter.jtgpath", JTGShared.DEFAULT_FILENAME);

            var compat = JTGShared.JTGCompatCheck(jtgPath);
            if (!compat.Compatible)
            {
                MessageBox.Show("Die gewählte Version von jTrainGraph ist wahrscheinlich nicht mit FPledit kompatibel. Bitte verwenden Sie jTrainGraph 2.02 - 2.03 oder 3.03 (und höher)!",
                    "jTrainGraphStarter: Fehler", MessageBoxType.Error);
                return;
            }
            if (pluginInterface.Timetable.Version != compat.Version && pluginInterface.Timetable.Type != TimetableType.Network)
            {
                MessageBox.Show("Die gewählte Version von jTrainGraph ist wahrscheinlich nicht mit der aktuellen Fahrplandatei kompatibel.",
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
                try
                {
                    p.Start();
                    pluginInterface.Logger.Info("Wartet darauf, dass jTrainGraph beendet wird...");
                    p.WaitForExit();
                    pluginInterface.Logger.Info("jTrainGraph beendet! Lade Datei neu...");

                    if (p.ExitCode != 0)
                        throw new Exception("Process exited with error code " + p.ExitCode);
                }
                catch (Exception e)
                {
                    pluginInterface.Logger.Error("Fehler beim Starten von jTrainGraph: Möglicherweise ist das jTrainGraphStarter Plugin falsch konfiguriert! Zur Konfiguration siehe jTrainGraph > Einstellungen");
                    pluginInterface.Logger.LogException(e);
                    return;
                }

                finished();

            }
            catch (Exception e)
            {
                pluginInterface.Logger.Error("jTrainGraphStarter: " + e.Message);
                pluginInterface.Logger.LogException(e);
            }
        }
    }
}

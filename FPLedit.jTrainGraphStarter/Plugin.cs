using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;

namespace FPLedit.jTrainGraphStarter
{
    [Plugin("Starter für jTrainGraph", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class Plugin : IPlugin
    {
        private IPluginInterface pluginInterface;
        private ButtonMenuItem startItem;

        public void Init(IPluginInterface pi, IComponentRegistry componentRegistry)
        {
            pluginInterface = pi;
            pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;
            
#pragma warning disable CA2000
            var item = ((MenuBar)pluginInterface.Menu).CreateItem(T._("&jTrainGraph"));
#pragma warning restore CA2000

            startItem = item.CreateItem(T._("jTrain&Graph starten"), enabled: false);
            startItem.Click += (s, e) =>
            {
                if (pluginInterface.Timetable.Type == TimetableType.Linear)
                    StartLinear();
                else
                    StartNetwork(pluginInterface.FileState.SelectedRoute);
            };

#pragma warning disable CA2000
            item.CreateItem(T._("Einstell&ungen"), clickHandler: (s, e) => (new SettingsForm(pluginInterface.Settings)).ShowModal(pluginInterface.RootForm));
#pragma warning restore CA2000
        }

        private void PluginInterface_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            startItem.Enabled = e.FileState.Opened;

            startItem.Text = (e.FileState.Opened && pluginInterface.Timetable.Type == TimetableType.Network) ?
                T._("jTrain&Graph starten (aktuelle Route)") : T._("jTrain&Graph starten");
        }

        private void StartLinear()
        {
            var backupHandle = pluginInterface.BackupTimetable();
            try
            {
                var showMessage = pluginInterface.Settings.Get("jTGStarter.show-message", true);

                if (showMessage)
                {
                    var res = MessageBox.Show(T._("Dies speichert die Fahrplandatei am letzten Speicherort und öffnet dann jTrainGraph. Nachdem Sie die Arbeit in jTrainGraph beendet haben, speichern Sie damit die Datei und schließen das jTrainGraph-Hauptfenster, damit werden die Änderungen übernommen. Aktion fortsetzen?\n\nDiese Meldung kann unter jTrainGraph > Einstellungen deaktiviert werden."),
                        T._("jTrainGraph starten"), MessageBoxButtons.YesNo, MessageBoxType.Warning);

                    if (res != DialogResult.Yes)
                        return;
                }

                pluginInterface.Save(false);

                StartJtg(pluginInterface.FileState.FileName, () => pluginInterface.Reload());
                pluginInterface.ClearBackup(backupHandle);
            }
            catch (Exception e)
            {
                pluginInterface.Logger.Error(T._("Beim Verwenden von jTrainGraph ist ein unerwarteter Fehler aufgetreten! {0}", e));
                pluginInterface.Logger.LogException(e);
                pluginInterface.RestoreTimetable(backupHandle);
            }
        }

        private void StartNetwork(int route)
        {
            var backupHandle = pluginInterface.BackupTimetable();
            try
            {
                var showMessage = pluginInterface.Settings.Get("jTGStarter.show-message", true);

                if (showMessage)
                {
                    DialogResult res = MessageBox.Show(T._("Dies speichert die aktuell ausgewählte Route in eine temporäre Datei und öffnet dann jTrainGraph. Nachdem Sie die Arbeit in jTrainGraph beendet haben, speichern Sie damit die Datei und schließen das jTrainGraph-Hauptfenster, damit werden alle Änderungen an den Bildfahrplaneinstellungen übernommen."
                        + "\nACHTUNG: Es werden nur Änderungen an der Bildfahrplandarstellung übernommen, alle anderen Änderungen (z.B. Bahnhöfe oder Züge einfügen) werden verworfen! Aktion fortsetzen?"
                        + "\n\nDiese Meldung kann unter jTrainGraph > Einstellungen deaktiviert werden."),
                        T._("jTrainGraph starten (aktuelle Route)"), MessageBoxButtons.YesNo, MessageBoxType.Warning);

                    if (res != DialogResult.Yes)
                        return;
                }

                var targetVersion = pluginInterface.Settings.GetEnum("jTGStarter.target-version", JtgShared.DEFAULT_TT_VERSION);
                
                if (targetVersion.GetVersionCompat().Compatibility != TtVersionCompatType.ReadWrite)
                    throw new Exception(T._("Zielversion ist nicht R/W-Kompatibel. Bitte die Einstellungen überprüfen."));

                var exporter = new Shared.Filetypes.XMLExport();
                var importer = new Shared.Filetypes.XMLImport();
                var sync = new TimetableRouteSync(pluginInterface.Timetable, route);
                var rtt = sync.GetRouteTimetable(targetVersion);
                var fn = pluginInterface.GetTemp(Guid.NewGuid().ToString() + "-route-" + route + ".fpl");
                exporter.SafeExport(rtt, fn, pluginInterface);

                StartJtg(fn, () =>
                {
                    pluginInterface.StageUndoStep();
                    var crtt = importer.SafeImport(fn, pluginInterface, new SilentLogger(pluginInterface.Logger));
                    sync.SyncBack(crtt);
                    pluginInterface.SetUnsaved();
                });
                pluginInterface.ClearBackup(backupHandle);
            }
            catch (Exception e)
            {
                pluginInterface.Logger.Error(T._("Beim Verwenden von jTrainGraph ist ein unerwarteter Fehler aufgetreten! {0}", e));
                pluginInterface.Logger.LogException(e);
                pluginInterface.RestoreTimetable(backupHandle);
            }
        }

        private void StartJtg(string fnArg, Action finished)
        {
            var javapath = pluginInterface.Settings.Get("jTGStarter.javapath", "java");
            var jtgPath = pluginInterface.Settings.Get("jTGStarter.jtgpath", JtgShared.DEFAULT_FILENAME);

            var compat = JtgShared.JtgCompatCheck(jtgPath, out var compatVersion);
            if (!compat)
            {
                MessageBox.Show(T._("Die gewählte Version von jTrainGraph ist wahrscheinlich nicht mit FPledit kompatibel. Bitte verwenden Sie jTrainGraph in einer kompatiblen Version!"),
                    T._("jTrainGraphStarter: Fehler"), MessageBoxType.Error);
                return;
            }
            if (pluginInterface.Timetable.Version != compatVersion && pluginInterface.Timetable.Type != TimetableType.Network)
            {
                MessageBox.Show(T._("Die gewählte Version von jTrainGraph ist wahrscheinlich nicht mit der aktuellen Fahrplandatei kompatibel."),
                    T._("jTrainGraphStarter: Fehler"), MessageBoxType.Error);
                return;
            }

            if (!ExecuteJTrainGraph(fnArg, jtgPath, javapath)) 
                return;

            try
            {
                finished();
            }
            catch (Exception e)
            {
                pluginInterface.Logger.Error("jTrainGraphStarter: " + e.Message);
                pluginInterface.Logger.LogException(e);
            }
        }

        private bool ExecuteJTrainGraph(string fnArg, string jtgPath, string javapath)
        {
            using (var runForm = new RunningForm(pluginInterface, fnArg, jtgPath, javapath))
            {
                runForm.ShowModal();
                return runForm.JtgSuccess;
            }
        }
    }
}

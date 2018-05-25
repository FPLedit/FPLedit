﻿using Eto.Forms;
using FPLedit.Shared;
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

            var item = MenuItem("jTrainGraph");
            info.Menu.Items.Add(item);

            startItem = MenuItem("jTrainGraph starten", item);
            startItem.Enabled = false;
            startItem.Click += (s, e) =>
            {
                if (info.Timetable.Type == TimetableType.Linear)
                    StartLinear();
                else
                    StartNetwork(info.FileState.SelectedRoute);
            };

            settingsItem = MenuItem("Einstellungen", item);
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
            //HACK: Remove when jTG supports opening again
            if (info.Settings.Get("jTGStarter.target-version", 009) == 009)
            {
                MessageBox.Show("ACHTUNG: jTrainGraph in der Version 3.0 ist derzeit auf Grund eines Fehlers nicht aus FPLedit heraus aufrufbar. Bitte speichern Sie die Datei in FPLedit, nehmen ihre Änderungen in jTRainGraph vor und öffnen diese anschließend wieder in FPLedit.", "jTrainGraph starten", MessageBoxType.Error);
                return;
            }

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
            //HACK: Remove when jTG supports opening again
            if (info.Settings.Get("jTGStarter.target-version", 009) == 009)
            {
                MessageBox.Show("ACHTUNG: jTrainGraph in der Version 3.0 ist derzeit auf Grund eines Fehlers nicht aus FPLedit heraus aufrufbar. Es ist derzeit auch keine Lösung des Problems verfügbar.", "jTrainGraph starten", MessageBoxType.Error);
                return;
            }

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

            int targetVersion = info.Settings.Get("jTGStarter.target-version", 009);

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
            string jtgPath = info.Settings.Get("jTGStarter.jtgpath", "jTrainGraph_302.jar");

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

        #region EtoHelpers
        private ButtonMenuItem MenuItem(string text)
        {
            var itm = new ButtonMenuItem();
            itm.Text = text;
            return itm;
        }

        private ButtonMenuItem MenuItem(string text, ButtonMenuItem parent)
        {
            var itm = new ButtonMenuItem();
            itm.Text = text;
            parent.Items.Add(itm);
            return itm;
        }
        #endregion
    }
}

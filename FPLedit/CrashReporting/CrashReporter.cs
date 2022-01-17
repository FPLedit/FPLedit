using Eto.Forms;
using FPLedit.Shared;
using System;
using System.IO;

namespace FPLedit.CrashReporting
{
    internal sealed class CrashReporter
    {
        private const string CRASH_DIR = "crash/";
        private const string REPORT_DIR = CRASH_DIR + "report/";
        private const string CRASH_TT_FN = REPORT_DIR + "crash_tt.fpl";
        private const string CRASH_REPORT_FN = REPORT_DIR + "crash_report.xml";
        private const string CRASH_FN_FN = CRASH_DIR + "crash.file";
        private const string CRASH_FLAG_FILE = CRASH_DIR + "crash.flag";

        private readonly IPluginInterface pluginInterface;

        public CrashReporter(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Report(CrashReport report)
        {
            try
            {
                var reportText = report.Serialize();
                var dir = pluginInterface.GetTemp(REPORT_DIR);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var fnTimetable = pluginInterface.GetTemp(CRASH_TT_FN);
                if (pluginInterface.Timetable != null)
                    new Shared.Filetypes.XMLExport().SafeExport(pluginInterface.Timetable, fnTimetable, pluginInterface);
                else if (File.Exists(fnTimetable))
                    File.Delete(fnTimetable);

                var fnReport = pluginInterface.GetTemp(CRASH_REPORT_FN);
                File.WriteAllText(fnReport, reportText);

                var fnCrashFileNameFile = pluginInterface.GetTemp(CRASH_FN_FN);
                File.WriteAllText(fnCrashFileNameFile, pluginInterface.FileState.FileName);

                var fnCrashFlag = pluginInterface.GetTemp(CRASH_FLAG_FILE);
                File.WriteAllText(fnCrashFlag, "1");

                MessageBox.Show(T._("Es ist ein unerwarteter Fehler in FPLedit aufgetreten.\n\n" +
                                "FPLedit wird neu gestartet. Möglicherweise ist eine Wiederherstellung möglich."), "FPLedit", MessageBoxType.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(T._("Es ist ein unerwarteter Fehler in FPLedit aufgetreten. Es konnten keine weiteren Informationen gespeichert werden. FPLedit wird neu gestartet.\n\n\n{0}", ex.Message), "FPLedit");
            }
        }

        public void Restore(FileHandler fh)
        {
            if (!HasCurrentTtBackup)
                return;

            var fileStateRestored = false;
            void RestoreFileState(object sender, EventArgs e)
            {
                if (fileStateRestored) return;
                fileStateRestored = true;
                fh.FileOpened -= RestoreFileState;
                
                if (!HasCurrentTtBackup) return;
                fh.FileState.Saved = false;
                fh.FileState.FileName = OrigTtFileName != "" ? OrigTtFileName : null;
                RemoveCrashFlag();
            }
            fh.FileOpened += RestoreFileState;
            
            fh.InternalOpen(CrashTtFileName, false);
        }

        // Crash flag
        public bool HasCurrentReport => File.Exists(pluginInterface.GetTemp(CRASH_FLAG_FILE));

        public void RemoveCrashFlag()
        {
            try { File.Delete(pluginInterface.GetTemp(CRASH_FLAG_FILE)); }
            catch { }
        }


        // Timetable backup after Crash
        public bool HasCurrentTtBackup => HasCurrentReport && File.Exists(pluginInterface.GetTemp(CRASH_TT_FN));

        public string CrashTtFileName => HasCurrentTtBackup ? pluginInterface.GetTemp(CRASH_TT_FN) : throw new NotSupportedException("No active timetable backup!");

        public string OrigTtFileName => HasCurrentTtBackup ? File.ReadAllText(pluginInterface.GetTemp(CRASH_FN_FN)) : throw new NotSupportedException("No active timetable backup!");

        // Report file
        public string ReportFn => HasCurrentReport ? pluginInterface.GetTemp(CRASH_REPORT_FN) : throw new NotSupportedException("No active crash report!");
    }
}

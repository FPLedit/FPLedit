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

                var fnTimetable = pluginInterface.GetTemp(REPORT_DIR + "crash_tt.fpl");
                if (pluginInterface.Timetable != null)
                    new Shared.Filetypes.XMLExport().SafeExport(pluginInterface.Timetable, fnTimetable, pluginInterface);
                else if (File.Exists(fnTimetable))
                    File.Delete(fnTimetable);

                var fnReport = pluginInterface.GetTemp(REPORT_DIR + "crash_report.xml");
                File.WriteAllText(fnReport, reportText);

                var fnCrashFileNameFile = pluginInterface.GetTemp(CRASH_DIR + "crash.file");
                File.WriteAllText(fnCrashFileNameFile, pluginInterface.FileState.FileName);

                var fnCrashFlag = pluginInterface.GetTemp(CRASH_DIR + "crash.flag");
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
            fh.InternalOpen(CrashTtFileName, false);
            fh.FileState.Saved = false;
            fh.FileState.FileName = OrigTtFileName != "" ? OrigTtFileName : null;
        }

        // Crash flag
        public bool HasCurrentReport => File.Exists(pluginInterface.GetTemp(CRASH_DIR + "crash.flag"));

        public void RemoveCrashFlag()
        {
            try
            {
                File.Delete(pluginInterface.GetTemp(CRASH_DIR + "crash.flag"));
            }
            catch
            {
            }
        }


        // Timetable backup after Crash
        public bool HasCurrentTtBackup => HasCurrentReport && File.Exists(pluginInterface.GetTemp(REPORT_DIR + "crash_tt.fpl"));

        public string CrashTtFileName => HasCurrentTtBackup ? pluginInterface.GetTemp(REPORT_DIR + "crash_tt.fpl") : throw new NotSupportedException();

        public string OrigTtFileName => HasCurrentTtBackup ? File.ReadAllText(pluginInterface.GetTemp(CRASH_DIR + "crash.file")) : throw new NotSupportedException();

        // Repor file
        public string ReportFn => HasCurrentReport ? pluginInterface.GetTemp(REPORT_DIR + "crash_report.xml") : throw new NotSupportedException();

        private string SL(string x)
        {
            return x;
        }
    }
}

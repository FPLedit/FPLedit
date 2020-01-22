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
            var reportText = report.Serialize();
            try
            {
                var dir = pluginInterface.GetTemp(REPORT_DIR);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var fn_tt = pluginInterface.GetTemp(REPORT_DIR + "crash_tt.fpl");
                if (pluginInterface.Timetable != null)
                    new Shared.Filetypes.XMLExport().Export(pluginInterface.Timetable, fn_tt, pluginInterface);
                else if (File.Exists(fn_tt))
                    File.Delete(fn_tt);

                var fn_report = pluginInterface.GetTemp(REPORT_DIR + "crash_report.xml");
                File.WriteAllText(fn_report, reportText);

                var fn_fileinfo = pluginInterface.GetTemp(CRASH_DIR + "crash.file");
                File.WriteAllText(fn_fileinfo, pluginInterface.FileState.FileName);

                var fn_crash = pluginInterface.GetTemp(CRASH_DIR + "crash.flag");
                File.WriteAllText(fn_crash, "1");

                MessageBox.Show("Es ist ein unerwarteter Fehler in FPLedit aufgetreten." + Environment.NewLine + Environment.NewLine +
                    "FPLedit wird neu gestartet. Möglicherweise ist eine Wiederherstellung möglich.", MessageBoxType.Error);
            }
            catch
            {
                MessageBox.Show("Es ist ein unerwarteter Fehler in FPLedit aufgetreten. Es konnten keine weiteren Informationen gespeichert werden. FPLedit wird neu gestartet.");
            }
        }

        public void Restore(FileHandler fh)
        {
            if (!HasCurrentTtBackup)
                return;
            fh.InternalOpen(CrashTtFileName);
            fh.FileState.Saved = false;
            fh.FileState.FileName = OrigTtFileName != "" ? OrigTtFileName : null;
        }

        // Crash flag
        public bool HasCurrentReport => File.Exists(pluginInterface.GetTemp(CRASH_DIR + "crash.flag"));

        public void RemoveCrashFlag() => File.Delete(pluginInterface.GetTemp(CRASH_DIR + "crash.flag"));


        // Timetable backup after Crash
        public bool HasCurrentTtBackup => HasCurrentReport && File.Exists(pluginInterface.GetTemp(REPORT_DIR + "crash_tt.fpl"));

        public string CrashTtFileName => HasCurrentTtBackup ? pluginInterface.GetTemp(REPORT_DIR + "crash_tt.fpl") : throw new NotSupportedException();

        public string OrigTtFileName => HasCurrentTtBackup ? File.ReadAllText(pluginInterface.GetTemp(CRASH_DIR + "crash.file")) : throw new NotSupportedException();

        // Repor file
        public string ReportFn => HasCurrentReport ? pluginInterface.GetTemp(REPORT_DIR + "crash_report.xml") : throw new NotSupportedException();
    }
}

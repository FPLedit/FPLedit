using Eto.Forms;
using FPLedit.Extensibility;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.CrashReporting
{
    internal class CrashReporter
    {
        private const string CRASH_DIR = "crash/";
        private const string REPORT_DIR = CRASH_DIR + "report/";

        private IInfo info;

        public CrashReporter(IInfo info)
        {
            this.info = info;
        }

        public void Report(CrashReport report)
        {
            var reporText = report.Serialize();
            try
            {
                var dir = info.GetTemp(REPORT_DIR);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var fn_tt = info.GetTemp(REPORT_DIR + "crash_tt.fpl");
                if (info.Timetable != null)
                    new Shared.Filetypes.XMLExport().Export(info.Timetable, fn_tt, info);
                else if (File.Exists(fn_tt))
                    File.Delete(fn_tt);

                var fn_report = info.GetTemp(REPORT_DIR + "crash_report.xml");
                File.WriteAllText(fn_report, reporText);

                var fn_fileinfo = info.GetTemp(CRASH_DIR + "crash.file");
                File.WriteAllText(fn_fileinfo, info.FileState.FileName);

                var fn_crash = info.GetTemp(CRASH_DIR + "crash.flag");
                File.WriteAllText(fn_crash, "1");

                MessageBox.Show("Es ist ein unerwarteter Fehler in FPLedit aufgetreten." + Environment.NewLine + Environment.NewLine +
                    "FPLedit wird neu gestartet. Möglicherweise ist eine Wiederherstellung möglich.", MessageBoxType.Error);
            }
            catch
            {
                MessageBox.Show("Es ist ein unerwarteter Fehler in FPLedit aufgetreten. Es konnten keine weiteren Informationen gespeichert werden. FPLedit wird neu gestartet.");
            }
        }

        public void Restore()
        {
            var form = (MainForm)info;
            if (!HasCurrentTtBackup)
                return;
            form.InternalOpen(CrashTtFileName);
            var fs = (FileState)form.FileState;
            fs.Saved = false;
            fs.FileName = OrigTtFileName != "" ? OrigTtFileName : null;
        }

        // Crash flag
        public bool HasCurrentReport => File.Exists(info.GetTemp(CRASH_DIR + "crash.flag"));

        public void RemoveCrashFlag() => File.Delete(info.GetTemp(CRASH_DIR + "crash.flag"));


        // Timetable backup after Crash
        public bool HasCurrentTtBackup => HasCurrentReport && File.Exists(info.GetTemp(REPORT_DIR + "crash_tt.fpl"));

        public string CrashTtFileName => HasCurrentTtBackup ? info.GetTemp(REPORT_DIR + "crash_tt.fpl") : throw new NotSupportedException();

        public string OrigTtFileName => HasCurrentTtBackup ? File.ReadAllText(info.GetTemp(CRASH_DIR + "crash.file")) : throw new NotSupportedException();

        // Repor file
        public string ReportFn => HasCurrentReport ? info.GetTemp(REPORT_DIR + "crash_report.xml") : throw new NotSupportedException();
    }
}

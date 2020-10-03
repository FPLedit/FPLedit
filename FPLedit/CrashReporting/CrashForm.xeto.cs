using Eto.Forms;
using FPLedit.Shared.UI;
using System;
using System.IO;
using FPLedit.Shared;

namespace FPLedit.CrashReporting
{
    internal sealed class CrashForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly Label fnLabel;
        private readonly TextArea infoTextBox;
        private readonly GroupBox restoreGroupBox;
        private readonly Button norestoreButton;
#pragma warning restore CS0649

        private readonly string crash_fn;

        public CrashForm(CrashReporter reporter)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            crash_fn = reporter.ReportFn;
            infoTextBox.Text = File.ReadAllText(crash_fn);
            infoTextBox.Selection = new Range<int>(0, 0);

            if (reporter.HasCurrentTtBackup)
                fnLabel.Text = reporter.OrigTtFileName != "" ? reporter.OrigTtFileName : T._("<Neue Datei>");
            restoreGroupBox.Visible = reporter.HasCurrentTtBackup;
            norestoreButton.Visible = !reporter.HasCurrentTtBackup;
        }

        private void MailButton_Click(object sender, EventArgs e) => OpenHelper.Open("mailto:info@manuelhu.de?subject=Bugreport%20FPLedit");

        private void FolderButton_Click(object sender, EventArgs e) => OpenHelper.Open(Path.GetDirectoryName(crash_fn));

        private void RestoreButton_Click(object sender, EventArgs e) => Close(DialogResult.Ok);

        private void NorestoreButton_Click(object sender, EventArgs e) => Close(DialogResult.Cancel);

        public static class L
        {
            public static readonly string Title = T._("Fehler in FPLedit");
            public static readonly string Description = T._("In FPLedit ist ein interner Fehler aufgetreten. Weiterführende Informationen wurden gesammelt:");
            public static readonly string ReportGroup = T._("Fehler melden");
            public static readonly string ReportDescription = T._("Diese können an den Autor von FPLedit weitergegeben werden, um zur Fehlerbehebung beizutragen.");
            public static readonly string Mail = T._("(1) Email senden und Fehlerhergang beschreiben");
            public static readonly string Folder = T._("(2) Dateien dieses Ordners anhängen");
            public static readonly string Thanks = T._("Vielen Dank, Ihr Programmautor");
            public static readonly string RestoreGroup = T._("Wiederherstellung");
            public static readonly string RestoreDescription = T._("Der Zustand des bearbeiteten Fahrplans wurde beim Absturz gespeichert. Möglicherweise können die Daten wiederhergestellt werden:");
            public static readonly string Restore = T._("Datei wiederherstellen");
            public static readonly string Cancel = T._("Nicht wiederherstellen");
            public static readonly string Close = T._("Schließen");
        }
    }
}

using Eto.Forms;
using FPLedit.Shared.UI;
using System;
using System.IO;
using FPLedit.Shared;

namespace FPLedit.CrashReporting
{
    internal sealed class CrashForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649,CA2213
        private readonly Label fnLabel = default!;
        private readonly TextArea infoTextBox = default!;
        private readonly GroupBox restoreGroupBox = default!;
        private readonly Button norestoreButton = default!;
#pragma warning restore CS0649,CA2213

        private readonly string crashFn;

        public CrashForm(CrashReporter reporter)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            crashFn = reporter.ReportFn;
            infoTextBox.Text = File.ReadAllText(crashFn);
            infoTextBox.Selection = new Range<int>(0, 0);

            if (reporter.HasCurrentTtBackup)
                fnLabel.Text = reporter.OrigTtFileName != "" ? reporter.OrigTtFileName : T._("<Neue Datei>");
            restoreGroupBox.Visible = reporter.HasCurrentTtBackup;
            norestoreButton.Visible = !reporter.HasCurrentTtBackup;
        }

        private void MailButton_Click(object sender, EventArgs e) => OpenHelper.Open("mailto:info@manuelhu.de?subject=Bugreport%20FPLedit");

        private void FolderButton_Click(object sender, EventArgs e) => OpenHelper.Open(Path.GetDirectoryName(crashFn)!);

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

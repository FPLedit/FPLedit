using Eto.Forms;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FPLedit.CrashReporting
{
    internal class CrashForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly Label fnLabel;
        private readonly TextArea infoTextBox;
        private readonly GroupBox restoreGroupBox;
        private readonly Button norestoreButton;
#pragma warning restore CS0649

        private string crash_fn;

        public CrashForm(CrashReporter reporter)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            crash_fn = reporter.ReportFn;
            infoTextBox.Text = File.ReadAllText(crash_fn);
            infoTextBox.Selection = new Range<int>(0, 0);

            if (reporter.HasCurrentTtBackup)
                fnLabel.Text = reporter.OrigTtFileName != "" ? reporter.OrigTtFileName : "<Neue Datei>";
            restoreGroupBox.Visible = reporter.HasCurrentTtBackup;
            norestoreButton.Visible = !reporter.HasCurrentTtBackup;
        }

        private void mailButton_Click(object sender, EventArgs e) => Process.Start("mailto:info@manuelhu.de?subject=Bugreport%20FPLedit");

        private void folderButton_Click(object sender, EventArgs e) => Process.Start(Path.GetDirectoryName(crash_fn));

        private void restoreButton_Click(object sender, EventArgs e) => Close(DialogResult.Ok);

        private void norestoreButton_Click(object sender, EventArgs e) => Close(DialogResult.Cancel);
    }
}

using Eto.Forms;
using FPLedit.Shared;
using System;
using FPLedit.Shared.UI;

namespace FPLedit.DebugDump.Forms
{
    internal class SettingsForm : FDialog<DialogResult>
    {
        private readonly ISettings settings;

#pragma warning disable CS0649
        private readonly TextBox pathTextBox;
        private readonly CheckBox recordCheckBox;
        private readonly Label helpLabel;
#pragma warning restore CS0649

        public SettingsForm(ISettings settings)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.settings = settings;

            pathTextBox.Text = settings.Get("dump.path", "");
            recordCheckBox.Checked = settings.Get("dump.record", false);
            
            helpLabel.WordWrap(550);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            settings.Set("dump.record", recordCheckBox.Checked.Value);
            settings.Set("dump.path", pathTextBox.Text);
            Close();
        }

        private void SelectTargetDir_Click(object sender, EventArgs e)
        {
            using (var sfd = new SelectFolderDialog())
            {
                sfd.Directory = pathTextBox.Text;
                if (sfd.ShowDialog(this) == DialogResult.Ok)
                {
                    pathTextBox.Text = sfd.Directory;
                }
            }
        }

        private void ViewDump_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.AddLegacyFilter("*.fpldmp|*.fpldmp");
                if (ofd.ShowDialog(this) == DialogResult.Ok)
                {
                    try
                    {

                        var reader = new DumpReader(ofd.FileName);
                        var events = reader.Events;
                        using (var isf = new InspectForm(events))
                            isf.ShowModal();
                    }
                    catch
                    {
                        MessageBox.Show("Fehler beim Öffnen der Datei.");
                    }
                }
            }
        }
    }
}

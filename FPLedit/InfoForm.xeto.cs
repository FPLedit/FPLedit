using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared.UI;
using System;
using System.IO;

namespace FPLedit
{
    internal sealed class InfoForm : FDialog
    {
#pragma warning disable CS0649
        private readonly TextArea licenseTextArea, thirdPartyTextArea;
        private readonly Label versionLabel;
#pragma warning restore CS0649

        public InfoForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            using (var stream = this.GetResource("Resources.Info.txt"))
            using (var sr = new StreamReader(stream))
                licenseTextArea.Text = sr.ReadToEnd();
            
            using (var stream = this.GetResource("Resources.3rd-party.txt"))
            using (var sr = new StreamReader(stream))
                thirdPartyTextArea.Text = sr.ReadToEnd();

            versionLabel.Text = versionLabel.Text.Replace("{version}", VersionInformation.Current.DisplayVersion);
            versionLabel.Font = SystemFonts.Bold();
        }

        private void CloseButton_Click(object sender, EventArgs e) => Close();
    }
}

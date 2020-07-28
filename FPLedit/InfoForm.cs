using Eto.Drawing;
using Eto.Forms;
using System;

namespace FPLedit
{
    internal sealed class InfoForm : AboutDialog
    {
        public InfoForm()
        {
            License = ResourceHelper.GetStringResource("Resources.Info.txt") 
                      + ResourceHelper.GetStringResource("Resources.3rd-party.txt");
            Version = Vi.DisplayVersion;

            Logo = new Icon(ResourceHelper.GetResource("Resources.programm.ico"));
            ProgramName = "FPLedit";
            ProgramDescription = LocalizationHelper._("Erstellen von Fahrplanunterlagen für Modelleisenbahnen");
            
            WebsiteLabel = LocalizationHelper._("Online-Dokumentation & Updates");
            Website = new Uri("https://fahrplan.manuelhu.de/");

            this.Developers = new[] { "Manuel Huber https://www.manuelhu.de", LocalizationHelper._("alle FPLedit-Beta-Tester") };
        }
    }
}

﻿using Eto.Drawing;
using Eto.Forms;
using System;
using FPLedit.Shared;

namespace FPLedit
{
    internal sealed class InfoForm : AboutDialog
    {
        public InfoForm()
        {
            License = ResourceHelper.GetStringResource("Resources.Info.txt") 
                      + ResourceHelper.GetStringResource("Resources.3rd-party.txt").Replace("{eto_version}", Vi.EtoVersion);
            Version = Vi.DisplayVersion;

            Logo = new Icon(ResourceHelper.GetResource("Resources.programm.ico"));
            ProgramName = "FPLedit";
            ProgramDescription = T._("Erstellen von Fahrplanunterlagen für Modelleisenbahnen");
            
            WebsiteLabel = T._("Online-Dokumentation & Updates");
            Website = new Uri("https://fahrplan.manuelhu.de/");

            this.Developers = new[] { "Manuel Huber https://www.manuelhu.de", T._("alle FPLedit-Beta-Tester") };
        }
    }
}

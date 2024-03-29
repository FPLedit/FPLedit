﻿using FPLedit.Shared;
using System;
using System.IO;
using System.Linq;

namespace FPLedit.BackwardCompat;

internal sealed class LinearUpgradeExport : BaseUpgradeExport
{
    protected override Timetable PerformUpgrade(XMLEntity xclone, TimetableVersion origVersion, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null)
    {
        if (origVersion.GetVersionCompat().Type != TimetableType.Linear)
            throw new Exception(T._("Nur lineare Fahrplandateien können mit {0} aktualisiert werden!", nameof(LinearUpgradeExport)));
        if (origVersion.Compare(TimetableVersion.JTG3_3) >= 0)
            throw new Exception(T._("Nur Fahrpläne mit einer älteren Dateiversion können aktualisiert werden."));
        if (origVersion.CompareTo(TimetableVersion.JTG2_x) < 0)
            throw new Exception(T._("Dateiversion ist zu alt, um aktualisiert zu werden!"));

        xclone.SetAttribute("version", TimetableVersion.JTG3_3.ToNumberString());

        // UPGRADE 008 -> 009
        if (origVersion == TimetableVersion.JTG2_x)
        {
            var shv = xclone.GetAttribute<bool>("shV");
            xclone.SetAttribute("shV", shv ? "1" : "0");

            // km -> kml/kmr
            var sElm = xclone.Children.Single(c => c.XName == "stations");
            foreach (var sta in sElm.Children.Where(x => x.XName == "sta"))
            {
                var oldPosition = sta.GetAttribute("km", "");
                sta.SetAttribute("kml", oldPosition);
                sta.SetAttribute("kmr", oldPosition);
                sta.RemoveAttribute("km");
            }

            // Allocate train ids
            var tElm = xclone.Children.Single(c => c.XName == "trains");
            var tElms = tElm.Children.Where(t => t.XName == "ti" || t.XName == "ta").ToArray();
            var nextId = tElms.DefaultIfEmpty().Max(t => t?.GetAttribute("id", -1) ?? -1);
            foreach (var orig in tElms)
            {
                if (orig.GetAttribute("id", -1) == -1)
                    orig.SetAttribute("id", (++nextId).ToString());
            }
        }

        // UPGRADE 009 -> 010 is empty
        // UPGRADE 010 -> 011 is empty

        // UPGRADE 011 --> 012 (CURRENT)
        if (origVersion.CompareTo(TimetableVersion.JTG3_3) < 0)
            UpgradeTimePrecision(xclone, true);

        // UPGRADE GENERAL
        var ttclone = new Timetable(xclone);
        LegacyColorTimetableConverter.ConvertAll(ttclone);

        return ttclone;
    }
}
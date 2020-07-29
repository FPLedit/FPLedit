using FPLedit.Shared;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.TimetableChecks
{
    internal sealed class FixNetworkAttributesAction : ITimetableInitAction
    {
        public string Init(Timetable tt, IReducedPluginInterface pluginInterface)
        {
            // Bug in FPledit 1.5.4 bis 2.0.0 muss nachträglich korrigiert werden
            // Vmax/Wellenlinien bei Stationen wurden nicht routenspezifisch gespeichert
            if (tt.Type == TimetableType.Network)
            {
                List<Station> hadAttrsUpgrade = new List<Station>();
                string[] upgradeAttrs = new[] { "fpl-vmax", "fpl-wl", "tr" };
                foreach (var sta in tt.Stations)
                {
                    foreach (var attr in upgradeAttrs)
                    {
                        var val = sta.GetAttribute<string>(attr, null);
                        if (string.IsNullOrEmpty(val))
                            continue;
                        if (val.Contains(':'))
                            continue;

                        var r = sta.Routes.First();
                        sta.SetAttribute(attr, r + ":" + val);
                        hadAttrsUpgrade.Add(sta);
                    }
                }

                if (hadAttrsUpgrade.Any())
                {
                    return T._("Aufgrund eines Fehlers in früheren Versionen von FPLedit mussten leider einige Höchstgeschwindigkeiten und Wellenlinienangaben zurückgesetzt werden. Die betroffenen Stationen sind: {0}",
                        string.Join(", ", hadAttrsUpgrade.Distinct().Select(s => s.SName)));
                }
            }
            return null;
        }
    }
}

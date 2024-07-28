using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared;

namespace FPLedit.TimetableChecks;

public class BugFixInitAction : ITimetableInitAction
{
    public string? Init(Timetable tt, IReducedPluginInterface pluginInterface)
    {
        var upgradeMessages = new List<string>();

        // Bug in FPLedit 1.5.3 bis 2.0.0 muss nachträglich korrigiert werden
        // In manchen Fällen wurden Zug-Ids doppelt vergeben
        var duplicateTraIds = tt.Trains.OfType<IWritableTrain>().GroupBy(t => t.Id).Where(g => g.Count() > 1).Select(g => g.ToArray()).ToArray();
        if (duplicateTraIds.Any()) // Wir haben doppelte IDs
        {
            if (tt.Transitions.Any())
            {
                var duplicateTransitions = duplicateTraIds.Where(dup => tt.HasTransition(dup[0], false)).ToArray();
                foreach (var dup in duplicateTransitions)
                    tt.RemoveTransition(dup[0], false); // Transitions mit dieser Id entfernen

                if (duplicateTransitions.Any())
                    upgradeMessages.Add(T._("Aufgrund eines Fehlers in früheren Versionen von FPLedit mussten leider einige Verknüpfungen zu Folgezügen aufgehoben werden. Die betroffenen Züge sind: {0}",
                        string.Join(", ", duplicateTransitions.SelectMany(dup => dup.Select(t => t.TName)))));
            }

            // Korrektur ohne Side-Effects möglich, alle doppelten Zug-Ids werden neu vergeben
            foreach (var dup in duplicateTraIds)
            foreach (var t in dup.Skip(1))
                t.Id = tt.AssignNextTrainId();
        }

        // Bug in FPLedit 2.2.0 bis 2.6.2 erzeugte beim Verschieben von Stationen korrupte Zugläufe.
        var moveCorruptedTrains = tt.Trains.OfType<IWritableTrain>().Where(t => !new TrainPathData(tt, t).IsValidPathIntegrity()).Select(t => t.TName).ToArray();
        if (moveCorruptedTrains.Length > 0)
        {
            upgradeMessages.Add(T._("Aufgrund eines Fehlers in früheren Versionen von FPLedit sind einige Züge beschädigt. Die betroffenen Züge sind: {0}",
                string.Join(", ", moveCorruptedTrains)));
        }

        // Bug in FPLedit x.x.x bis 2.6.3 erzeugte beim Verschieben von Stationen eine korrupte lineare Strecke.
        if (tt.Type == TimetableType.Linear)
        {
            // check that we have a consistent ordering.
            var staElems = tt.XMLEntity.Children.Single(x => x.XName == "stations")
                .Children.Where(x => x.XName == "sta").ToList();
            var stations = tt.GetRoute(Timetable.LINEAR_ROUTE_ID).Stations.ToList(); // Replace collection with an ordered one
            var idx = 0;
            var hasInconsitentOrdering = false;
            foreach (var c in stations) // filter other xml elements.
                hasInconsitentOrdering |= (staElems.IndexOf(c.XMLEntity) != idx++);

            if (hasInconsitentOrdering)
            {
                upgradeMessages.Add(T._("Aufgrund eines Fehlers in früheren Versionen von FPLedit sind ist die Reihenfolge und Kilometrierung der Stationen nicht überein."));
            }
        }

        return upgradeMessages.Any() ? string.Join(Environment.NewLine, upgradeMessages) : null;
    }
}
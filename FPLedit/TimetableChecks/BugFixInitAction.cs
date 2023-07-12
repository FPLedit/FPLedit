using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared;

namespace FPLedit.TimetableChecks
{
    public class BugFixInitAction : ITimetableInitAction
    {
        private const string KEY_AMBIGUOUS = "bugfix.corrected.ambiguous-routes";
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

            // Bug in FPLedit 2.1 muss nachträglich klar gemacht werden.
            // Durch Nutzerinteraction konnten "ambiguous routes" entstehen.
            // Eine Korrektur ist nicht möglich.
            if (pluginInterface.Cache.Get(KEY_AMBIGUOUS) != "1")
            {
                if (tt.Type == TimetableType.Network && tt.HasRouteCycles)
                {
                    // All stations that are junction points.
                    var maybeAffectedRoutes = tt.GetCyclicRoutes();
                    var junctions = tt.Stations.Where(s => s.IsJunction && s.Routes.Intersect(maybeAffectedRoutes).Any()).ToArray();
                    var hasAmbiguousRoutes = tt.CheckAmbiguousRoutesInternal(junctions);

                    if (hasAmbiguousRoutes)
                        upgradeMessages.Add(T._("Die Datei enthält zusammengefallene Strecken, das heißt zwei Stationen sind auf mehr als einer Route ohne Zwischenstation verbunden. FPLedit kann sich danach komisch verhalten und Züge zufällig über die eine oder andere Strecke leiten. Eine Korrektur ist leider nicht möglich."));
                }
                pluginInterface.Cache.Set(KEY_AMBIGUOUS, "1");
            }

            return upgradeMessages.Any() ? string.Join(Environment.NewLine, upgradeMessages) : null;
        }
    }
}
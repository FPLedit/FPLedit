using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace FPLedit.Shared
{
    [Serializable]
    [DebuggerDisplay("{TTName}")]
    public sealed class Timetable : Entity, ITimetable
    {
        public const int LINEAR_ROUTE_ID = 0;
        public static TimetableVersion DefaultLinearVersion { get; set; } = TimetableVersion.JTG3_0;

        XMLEntity sElm, tElm, trElm;

        public TimetableVersion Version => (TimetableVersion)GetAttribute("version", 0);
        public TimetableType Type => Version == TimetableVersion.Extended_FPL ? TimetableType.Network : TimetableType.Linear;

        private int nextStaId = 0, nextRtId = 0, nextTraId = 0;

        public string TTName
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }

        private List<Station> stations;
        private List<Train> trains;
        private List<Transition> transitions;

        public List<Station> Stations => stations;

        public List<Train> Trains => trains;

        public List<Transition> Transitions => transitions;

        [field: NonSerialized]
        public string UpgradeMessage { get; private set; }

        public Timetable(TimetableType type) : base("jTrainGraph_timetable", null) // Root without parent
        {
            stations = new List<Station>();
            trains = new List<Train>();
            transitions = new List<Transition>();

            SetAttribute("version", type == TimetableType.Network ? "100" : DefaultLinearVersion.ToNumberString()); // version="100" nicht kompatibel mit jTrainGraph
            sElm = new XMLEntity("stations");
            tElm = new XMLEntity("trains");
            trElm = new XMLEntity("transitions");
            Children.Add(sElm);
            Children.Add(tElm);
        }

        public Timetable(XMLEntity en) : base(en, null)
        {
            if (Type == TimetableType.Network && Version != TimetableVersion.Extended_FPL)
                throw new Exception("Falsche Versionsummer für Netzwerk-Fahrplandatei!");

            if (!Enum.IsDefined(typeof(TimetableVersion), Version))
                throw new Exception("Unbekannte Dateiversion. Nur mit jTrainGraph 2 oder 3.0 erstellte Dateien können geöffnet werden!");

            stations = new List<Station>();
            sElm = Children.FirstOrDefault(x => x.XName == "stations");
            if (sElm != null)
            {
                foreach (var c in sElm.Children.Where(x => x.XName == "sta")) // Filtert andere Elemente
                    stations.Add(new Station(c, this));
            }
            else
            {
                sElm = new XMLEntity("stations");
                Children.Add(sElm);
            }

            trains = new List<Train>();
            tElm = Children.FirstOrDefault(x => x.XName == "trains");
            if (sElm == null && tElm != null)
                throw new Exception("Kein <stations>-Element vorhanden, dafür aber <trains>!");

            if (tElm != null)
            {
                var directions = Enum.GetNames(typeof(TrainDirection));
                foreach (var c in tElm.Children.Where(x => directions.Contains(x.XName))) // Filtert andere Elemente
                    trains.Add(new Train(c, this));
            }
            else
            {
                tElm = new XMLEntity("trains");
                Children.Add(tElm);
            }

            transitions = new List<Transition>();
            trElm = Children.FirstOrDefault(x => x.XName == "transitions");
            if (trElm != null)
            {
                foreach (var c in trElm.Children.Where(x => x.XName == "tra")) // Filtert andere Elemente
                    transitions.Add(new Transition(c, this));
            }
            else
            {
                trElm = new XMLEntity("transitions");
                Children.Add(trElm);
            }

            // Höchste IDs ermitteln
            if (trains.Count > 0)
                nextTraId = trains.Max(s => s.Id);
            if (Type == TimetableType.Network)
            {
                nextStaId = stations.Max(s => s.Id);
                nextRtId = stations.SelectMany(s => s.Routes).DefaultIfEmpty().Max();
            }

            var upgradeMessages = new List<string>();

            // BUG in FPLedit 1.5.3 bis 2.0.0 muss nachträglich korrigiert werden
            // In manchen Fällen wurden Zug-Ids doppelt vergeben
            var duplicate_tra_ids = trains.GroupBy(t => t.Id).Where(g => g.Count() > 1).Select(g => g.ToArray());
            if (duplicate_tra_ids.Any()) // Wir haben doppelte IDs
            {
                if (transitions.Count > 0)
                {
                    var duplicate_transitions = duplicate_tra_ids.Where(dup => HasTransition(dup[0], false)).ToArray();
                    foreach (var dup in duplicate_transitions)
                        RemoveTransition(dup[0], false); // Transitions mit dieser Id entfernen

                    if (duplicate_transitions.Any())
                        upgradeMessages.Add("Aufgrund eines Fehlers in früheren Versionen von FPLedit mussten leider einige Verknüpfungen zu Folgezügen aufgehoben werden. Die betroffenen Züge sind: "
                            + string.Join(", ", duplicate_transitions.SelectMany(dup => dup.Select(t => t.TName))));
                }

                // Korrektur ohne Side-Effects möglich, alle doppelten Zug-Ids werden neu vergeben
                foreach (var dup in duplicate_tra_ids)
                    dup.Skip(1).All((t) => { t.Id = ++nextTraId; return true; });
            }

            // Zügen ohne IDs diese neu zuweisen
            foreach (var train in trains)
            {
                if (train.Id == -1)
                    train.Id = ++nextTraId;
            }

            // Clean up invalid transitions
            var tids = trains.Select(t => t.Id).ToArray();
            foreach (var tra in Transitions.ToArray())
            {
                if (!tids.Contains(tra.First))
                    RemoveTransition(tra.First);
                else if (!tids.Contains(tra.Next))
                    RemoveTransition(tra.Next, false);
            }


            // BUG in FPledit 1.5.4 bis 2.0.0 muss nachträglich korrigiert werden
            // Vmax/Wellenlinien bei Stationen wurden nicht routenspezifisch gespeichert
            if (Type == TimetableType.Network)
            {
                List<Station> hadAttrsUpgrade = new List<Station>();
                string[] upgradeAttrs = new[] { "fpl-vmax", "fpl-wl", "tr" };
                foreach (var sta in Stations)
                {
                    foreach (var attr in upgradeAttrs)
                    {
                        var val = sta.GetAttribute<string>(attr, null);
                        if (val == null || val == "")
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
                    upgradeMessages.Add("Aufgrund eines Fehlers in früheren Versionen von FPLedit mussten leider einige Höchstgeschwindigkeiten und Wellenlinienangaben zurückgesetzt werden. Die betroffenen Stationen sind: "
                        + string.Join(", ", hadAttrsUpgrade.Select(s => s.SName)));
                }
            }

            UpgradeMessage = string.Join(Environment.NewLine, upgradeMessages);
            if (UpgradeMessage == "")
                UpgradeMessage = null;
            //TODO: Somehow re-enable after Shared.Rendering split-up
            //ColorTimetableConverter.ConvertAll(this);
        }

        public List<Station> GetStationsOrderedByDirection(TrainDirection direction = TrainDirection.ti)
        {
            float linearOrder(Station s) => s.Positions.GetPosition(LINEAR_ROUTE_ID).Value;

            if (Type == TimetableType.Network)
                throw new NotSupportedException("Netzwerk-Fahrpläne haben keine Richtung!");
            return (direction == TrainDirection.ta ?
                Stations.OrderByDescending(linearOrder)
                : Stations.OrderBy(linearOrder)).ToList();
        }

        public string GetLineName(TrainDirection direction)
        {
            if (Type == TimetableType.Network)
                throw new NotSupportedException("Netzwerk-Fahrpläne haben keine Richtung!");
            var stas = GetStationsOrderedByDirection(direction);
            return stas.First().SName + " - " + stas.Last().SName;
        }

        public Timetable Clone()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                return (Timetable)formatter.Deserialize(stream);
            }
        }

        #region Hilfsmethoden für andere Entitäten

        public void AddStation(Station sta, int route)
        {
            sta._parent = this;

            // Neue Id an Station vergeben
            if (Type == TimetableType.Network)
                sta.Id = ++nextStaId;

            stations.Add(sta);
            if (Type == TimetableType.Linear)
            {
                route = LINEAR_ROUTE_ID;
                stations = GetStationsOrderedByDirection();
                var idx = stations.IndexOf(sta); // Index vorläufig ermitteln

                // Es können ja noch andere Nodes in den Children sein.
                if (idx != 0)
                {
                    var staBefore = stations[idx - 1];
                    idx = sElm.Children.IndexOf(staBefore.XMLEntity) + 1;
                }
                else if (stations.Count > idx + 1)
                {
                    var staAfter = stations[idx + 1];
                    idx = sElm.Children.IndexOf(staAfter.XMLEntity); // Davor einfügen
                }
                sElm.Children.Insert(idx, sta.XMLEntity);
            }
            else
                sElm.Children.Add(sta.XMLEntity);

            // Auch bei allen Zügen hinzufügen
            foreach (var t in Trains)
                t.AddArrDep(sta, new ArrDep(), route);
        }

        public void RemoveStation(Station sta)
        {
            foreach (var train in Trains)
                train.RemoveArrDep(sta);

            var needsCleanup = stations.First() == sta || stations.Last() == sta;

            sta._parent = null;
            stations.Remove(sta);
            sElm.Children.Remove(sta.XMLEntity);

            if (!needsCleanup && Type == TimetableType.Linear)
                return;

            // Wenn Endstationen gelöscht werden könnten sonst korrupte Dateien entstehen!
            foreach (var train in Trains)
                train.RemoveOrphanedTimes();
        }

        public Station GetStationById(int id)
        {
            if (Type == TimetableType.Linear)
                throw new NotSupportedException("Lineare Strecken haben keine Stations-Ids!");
            return stations.FirstOrDefault(s => s.Id == id);
        }

        public void AddTrain(Train tra, bool hasArDeps = false)
        {
            tra.Id = ++nextTraId;

            if (!hasArDeps && Type == TimetableType.Linear)
                tra.AddLinearArrDeps();

            tra._parent = this;
            trains.Add(tra);
            tElm.Children.Add(tra.XMLEntity);
        }

        public Train GetTrainById(int id)
            => trains.FirstOrDefault(t => t.Id == id);

        public void RemoveTrain(Train tra)
        {
            RemoveTransition(tra, false); // Remove "orphaned" transitions

            tra._parent = null;
            trains.Remove(tra);
            tElm.Children.Remove(tra.XMLEntity);
        }

        #endregion

        #region Hilfsmethoden für Routen
        // "Eröffnet" eine neue Strecke zwischen zwei Bahnhöfen
        public int AddRoute(Station s_old, Station s_new, float old_add_km, float new_km)
        {
            if (Type == TimetableType.Linear)
                throw new NotSupportedException("Lineare Strecken haben keine Routen!");
            var idx = ++nextRtId;
            var r1 = s_old.Routes.ToList();
            var r2 = s_new.Routes.ToList();
            r1.Add(idx);
            r2.Add(idx);
            s_old.Routes = r1.ToArray();
            s_new.Routes = r2.ToArray();
            s_old.Positions.SetPosition(idx, old_add_km);
            s_new.Positions.SetPosition(idx, new_km);
            return idx;
        }

        public Route[] GetRoutes()
        {
            var routes = new List<Route>();
            if (Type == TimetableType.Network)
            {
                var routesIndices = Stations.SelectMany(s => s.Routes).Distinct();
                foreach (var ri in routesIndices)
                    routes.Add(GetRoute(ri));
            }
            else
                routes.Add(new Route() { Index = LINEAR_ROUTE_ID, Stations = Stations });
            return routes.ToArray();
        }

        public Route GetRoute(int index)
        {
            if (Type == TimetableType.Linear && index == LINEAR_ROUTE_ID)
                return new Route() { Index = LINEAR_ROUTE_ID, Stations = Stations };
            var stas = Stations.Where(s => s.Routes.Contains(index)).ToList();
            return new Route() { Index = index, Stations = stas };
        }

        public void JoinRoutes(int route, Station sta2, float newKm)
        {
            if (Type == TimetableType.Linear)
                throw new NotSupportedException("Lineare Strecken haben keine Routen!");
            var routes = sta2.Routes.ToList();
            routes.Add(route);
            sta2.Routes = routes.ToArray();
            sta2.Positions.SetPosition(route, newKm);
        }

        public bool HasRouteCycles
        {
            get
            {
                if (Type == TimetableType.Linear)
                    throw new NotSupportedException("Lineare Strecken haben keine Routen!");

                var junctions = Stations.Where(s => s.Routes.Count() > 1);
                var rids = junctions.SelectMany(s => s.Routes).ToList();

                int[] GetSingles() => rids.GroupBy(r => r).Where(g => g.Count() == 1).Select(g => g.Key).ToArray();

                int[] singles = GetSingles();
                while (singles.Any())
                {
                    for (int i = 0; i < singles.Length; i++)
                    {
                        var s = singles[i];
                        // Find junction
                        var j = junctions.FirstOrDefault(t => t.Routes.Contains(s));
                        var r = j.Routes;
                        if (r.Length == 2) // s und eine andere
                            rids.Remove(r.First(t => t != s));
                        rids.Remove(s);
                    }
                    singles = GetSingles();
                }

                return rids.Any();
            }
        }
        #endregion

        #region Hilfsmethoden für Umläufe
        public void AddTransition(Train first, Train next)
        {
            if (next == null) return;
            var transition = new Transition(this)
            {
                First = first.Id,
                Next = next.Id
            };
            trElm.Children.Add(transition.XMLEntity);
            transitions.Add(transition);
        }

        public void SetTransition(Train first, Train newNext)
        {
            var trans = transitions.Where(t => t.First == first.Id);

            if (trans.Count() == 0 && newNext != null)
                AddTransition(first, newNext);
            else if (trans.Count() > 1)
                throw new Exception("Mehr als eine Transition mit angegebenem ersten Zug gefunden!");
            else if (trans.Count() == 1)
            {
                if (newNext != null)
                    trans.First().Next = newNext.Id;
                else
                    RemoveTransition(first);
            }
        }

        public Train GetTransition(Train first) => GetTransition(first.Id);

        public Train GetTransition(int tid)
        {
            var trans = transitions.Where(t => t.First == tid);

            if (trans.Count() == 0)
                return null;
            if (trans.Count() > 1)
                throw new Exception("Mehr als eine Transition mit angegebenem ersten Zug gefunden!");

            return GetTrainById(trans.First().Next);
        }

        public IEnumerable<Train> GetTransitions(Train first)
        {
            Train tra = first;
            while ((tra = GetTransition(tra)) != null)
                yield return tra;
        }

        public void RemoveTransition(Train tra, bool onlyAsFirst = true) => RemoveTransition(tra.Id, onlyAsFirst);

        public void RemoveTransition(int tid, bool onlyAsFirst = true)
        {
            var trans = transitions.Where(t => t.First == tid || (!onlyAsFirst && t.Next == tid));
            foreach (var transition in trans)
                trElm.Children.Remove(transition.XMLEntity);
            transitions.RemoveAll(t => trans.Contains(t));
        }

        public bool HasTransition(Train tra, bool onlyAsFirst = true)
            => transitions.Any(t => t.First == tra.Id || (!onlyAsFirst && t.Next == tra.Id));
        #endregion

        [DebuggerStepThrough]
        public override string ToString()
            => string.Join(" | ", GetRoutes().Select(r => r.GetRouteName()));
    }
}

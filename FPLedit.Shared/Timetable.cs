using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        XMLEntity sElm, tElm, trElm;

        public TimetableType Type { get; private set; }
        private int nextStaId = 0, nextRtId = 0, nextTraId = 0;

        public string TTName
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }

        public TimetableVersion Version => (TimetableVersion)GetAttribute("version", 0);

        private List<Station> stations;
        private List<Train> trains;
        private List<Transition> transitions;

        public List<Station> Stations => stations;

        public List<Train> Trains => trains;

        public List<Transition> Transitions => transitions;

        public Timetable() : this(TimetableType.Linear)
        {

        }

        public Timetable(TimetableType type) : base("jTrainGraph_timetable", null) // Root without parent
        {
            Type = type;
            stations = new List<Station>();
            trains = new List<Train>();
            transitions = new List<Transition>();

            SetAttribute("version", type == TimetableType.Network ? "100" : "008"); // version="100" nicht kompatibel mit jTrainGraph
            sElm = new XMLEntity("stations");
            tElm = new XMLEntity("trains");
            trElm = new XMLEntity("transitions");
            Children.Add(sElm);
            Children.Add(tElm);
        }

        public Timetable(XMLEntity en, TimetableType type) : base(en, null)
        {
            Type = type;
            if (type == TimetableType.Network && Version != TimetableVersion.Extended_FPL)
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
            nextTraId = trains.Max(s => s.Id);
            if (Type == TimetableType.Network)
            {
                nextStaId = stations.Max(s => s.Id);
                nextRtId = stations.SelectMany(s => s.Routes).DefaultIfEmpty().Max();
            }

            // Zügen ohne IDs diese zuweisen
            foreach (var train in trains)
            {
                if (train.Id == -1)
                    train.Id = nextTraId++;
            }
        }

        public Timetable(XMLEntity en) : this(en, TimetableType.Linear) // Root without parent
        {
        }

        public List<Station> GetStationsOrderedByDirection(TrainDirection direction)
        {
            if (Type == TimetableType.Network)
                throw new NotSupportedException("Netzwerk-Fahrpläne haben keine Richtung!");
            return (direction == TrainDirection.ta ?
                Stations.OrderByDescending(s => s.LinearKilometre)
                : Stations.OrderBy(s => s.LinearKilometre)).ToList();
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
            // Neue Id an Station vergeben
            if (Type == TimetableType.Network)
                sta.Id = ++nextStaId;

            sta._parent = this;
            stations.Add(sta);
            if (Type == TimetableType.Linear)
            {
                route = LINEAR_ROUTE_ID;
                stations = stations.OrderBy(s => s.LinearKilometre).ToList();
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
            tra.Id = nextTraId++;

            if (!hasArDeps && Type == TimetableType.Linear)
                foreach (var sta in Stations)
                    tra.AddArrDep(sta, new ArrDep(), LINEAR_ROUTE_ID);

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
        #endregion

        #region Hilfsmethoden für Umläufe
        public void AddTransition(Train first, Train next)
        {
            var transition = new Transition(this)
            {
                First = first.Id,
                Next = next.Id
            };
            trElm.Children.Add(transition.XMLEntity);
        }

        public void SetTransition(Train first, Train newNext)
        {
            var trans = transitions.Where(t => t.First == first.Id);

            if (trans.Count() == 0)
                AddTransition(first, newNext);
            if (trans.Count() > 1)
                throw new Exception("Keine Transition mit angegebenem ersten Zug gefunden!");

            trans.First().Next = newNext.Id;
        }

        public Train GetTransition(Train first)
        {
            var trans = transitions.Where(t => t.First == first.Id);

            if (trans.Count() == 0)
                return null;
            if (trans.Count() > 1)
                throw new Exception("Keine Transition mit angegebenem ersten Zug gefunden!");

            return GetTrainById(trans.First().Next);
        }

        public void RemoveTransition(Train tra, bool onlyAsFirst = true)
        {
            var trans = transitions.Where(t => t.First == tra.Id || (onlyAsFirst && t.Next == tra.Id));
            foreach (var transition in trans)
            {
                trElm.Children.Remove(transition.XMLEntity);
                transitions.Remove(transition);
            }
        }
        #endregion

        public string[] GetAllTfzs()
        {
            return Trains
                .Select(t => t.Locomotive)
                .Distinct()
                .Where(s => s != "")
                .ToArray();
        }

        [DebuggerStepThrough]
        public override string ToString()
            => GetLineName(TrainDirection.ta);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Helpers
{
    /// <summary>
    /// Helper class for "perceived" directions in network timetables.
    /// </summary>
    [Templating.TemplateSafe]
    public sealed class NetworkHelper
    {
        private readonly Timetable tt;

        public NetworkHelper(Timetable tt)
        {
            this.tt = tt;
        }
        
        /// <summary>
        /// Returns all stations on all routes that are before or ahead of the current station.
        /// </summary>
        /// <exception cref="ArgumentException">The spcified train direction is <see cref="TrainDirection.tr"/>.</exception>
        public Station[] GetStationsInDir(TrainDirection direction, Station sta)
        {
            if (direction == TrainDirection.tr)
                throw new ArgumentException("Invalid (non-linear) train direction specified", nameof(direction));
            
            if (tt.Type == TimetableType.Linear)
            {
                var stas = tt.GetLinearStationsOrderedByDirection(direction);
                return stas.Skip(stas.IndexOf(sta) + 1).ToArray();
            }
            var stasAfter = new List<Station>();
            foreach (var rt in sta.Routes)
            {
                var route = tt.GetRoute(rt);
                var pos = sta.Positions.GetPosition(rt);
                var nextStations = direction == TrainDirection.ti ?
                    route.Stations.Where(s => s.Positions.GetPosition(rt) > pos) : // ti
                    route.Stations.Where(s => s.Positions.GetPosition(rt) < pos); // ta
                stasAfter.AddRange(nextStations);
            }
            return stasAfter.ToArray();
        }
        
        /// <summary>
        /// Get all trains that pass through the given station with the provided direction.
        /// </summary>
        /// <param name="trains">Pre-filtered collection for trains. Only those trains will be taken into account.</param>
        /// <param name="dir">Linear "perceived" direction.</param>
        /// <param name="sta"></param>
        /// <exception cref="ArgumentException">The spcified train direction is <see cref="TrainDirection.tr"/>.</exception>
        public IEnumerable<ITrain> GetTrains(IEnumerable<ITrain> trains, TrainDirection dir, Station sta)
        {
            if (dir == TrainDirection.tr)
                throw new ArgumentException("Invalid (non-linear) train direction specified", nameof(dir));
            
            var stasAfter = GetStationsInDir(dir, sta);
            var stasBefore = GetStationsInDir(dir == TrainDirection.ta ? TrainDirection.ti : TrainDirection.ta, sta);

            return trains.Where(t =>
            {
                var path = t.GetPath();

                if (!path.Contains(sta))
                    return false;

                var ardeps = t.GetArrDepsUnsorted();
                var isStopping = ardeps[sta].HasMinOneTimeSet;
                
                var nextStopStation = path.Where(s => stasAfter.Contains(s)).FirstOrDefault(s => ardeps[s].HasMinOneTimeSet);
                Station lastStopStation = null;
                if (nextStopStation == null || !isStopping)
                    lastStopStation = path.Where(s => stasBefore.Contains(s)).FirstOrDefault(s => ardeps[s].HasMinOneTimeSet);

                // We have a stop at this station, use time difference between first/last and current.
                if (isStopping)
                {
                    if (nextStopStation == null)
                    {
                        if (lastStopStation == null)
                            return false;
                        var lastStopTime = ardeps[lastStopStation].LastSetTime;
                        return lastStopTime < ardeps[sta].FirstSetTime;
                    }

                    var nextStopTime = ardeps[nextStopStation].FirstSetTime;
                    return nextStopTime > ardeps[sta].LastSetTime;
                }

                if (lastStopStation == null || nextStopStation == null)
                    return false; // We are (proven!) not running over this station.
                // Passthrough, use difference between first and last
                return ardeps[nextStopStation].FirstSetTime > ardeps[lastStopStation].LastSetTime;
            });
        }

        /// <summary>
        /// Get the direction the train passes through this station.
        /// </summary>
        /// <remarks>This direction may differ from train to train (at the same station) and from station to station (with the same train).</remarks>
        /// <returns>If null, the train does not pass through this station.</returns>
        public TrainDirection? GetTrainDirectionAtStation(ITrain train, Station sta)
        {
            var stasAfter = GetStationsInDir(TrainDirection.ti, sta);
            var stasBefore = GetStationsInDir(TrainDirection.ta, sta);
            
            var path = train.GetPath();

            if (!path.Contains(sta))
                return null;

            var ardeps = train.GetArrDepsUnsorted();
            var isStopping = ardeps[sta].HasMinOneTimeSet;
                
            var nextStopStation = path.Where(s => stasAfter.Contains(s)).FirstOrDefault(s => ardeps[s].HasMinOneTimeSet);
            Station lastStopStation = null;
            if (nextStopStation == null || !isStopping)
                lastStopStation = path.Where(s => stasBefore.Contains(s)).FirstOrDefault(s => ardeps[s].HasMinOneTimeSet);

            // We have a stop at this station, use time difference between first/last and current.
            if (isStopping)
            {
                if (nextStopStation == null)
                {
                    if (lastStopStation == null)
                        return null;
                    var lastStopTime = ardeps[lastStopStation].LastSetTime;
                    return lastStopTime < ardeps[sta].FirstSetTime ? TrainDirection.ti : TrainDirection.ta;
                }

                var nextStopTime = ardeps[nextStopStation].FirstSetTime;
                return nextStopTime > ardeps[sta].LastSetTime ? TrainDirection.ti : TrainDirection.ta;
            }

            if (lastStopStation == null || nextStopStation == null)
                return null; // We are (proven!) not running over this station.
            // Passthrough, use difference between first and last
            return ardeps[nextStopStation].FirstSetTime > ardeps[lastStopStation].LastSetTime ? TrainDirection.ti : TrainDirection.ta;
        }
    }
}
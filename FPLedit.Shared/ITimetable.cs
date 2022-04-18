using System;
using System.Collections.Generic;

namespace FPLedit.Shared
{
    /// <summary>
    /// Object model type representing a single timetable file state.
    /// </summary>
    [Templating.TemplateSafe]
    public interface ITimetable
    {
        /// <summary>
        /// This event will be fired when the trains of this timetable instance are changed.
        /// </summary>
        event EventHandler? TrainsXmlCollectionChanged;
        
        /// <summary>
        /// The general type of this timetable.
        /// </summary>
        TimetableType Type { get; }
        /// <summary>
        /// The file format version of this timetable file.
        /// </summary>
        TimetableVersion Version { get; }
        /// <summary>
        /// A read-only list of all stations associated with this timetbale.
        /// </summary>
        IList<Station> Stations { get; }
        /// <summary>
        /// A read-only list of all trains associated with this timetbale.
        /// </summary>
        IList<ITrain> Trains { get; }
        /// <summary>
        /// A read-only list of all transitions associated with this timetbale.
        /// </summary>
        IList<Transition> Transitions { get; }
        /// <summary>
        /// The display name of this timetable.
        /// </summary>
        string TTName { get; set; }
        /// <summary>
        /// This is the default time in minutes which a train stays on a track before departing or after arriving at it's first or last station.
        /// </summary>
        TimeEntry DefaultPrePostTrackTime { get; set; }
        
        /// <summary>
        /// This method adds a non-existent <see cref="Station"/> to an existing route in this timetable.
        /// </summary>
        /// <remarks>Adding an already added station will throw, as well as using a non-zero Route index on a linear timetable.</remarks>
        void AddStation(Station sta, int route);
        /// <summary>
        /// Retrieve a station by it's unique id. This only works in network timetables.
        /// </summary>
        Station? GetStationById(int id);
        /// <summary>
        /// This method removes the given Station from this timetable instance.
        /// </summary>
        void RemoveStation(Station sta);

        /// <summary>
        /// This method adds a non-existent <see cref="ITrain"/> to this timetable.
        /// </summary>
        /// <remarks>Adding an already added train will do nothing</remarks>
        void AddTrain(ITrain tra);
        /// <summary>
        /// Retrieve a train by it's (not neccessaryly) unique id. This only works in network timetables.
        /// </summary>
        ITrain? GetTrainById(int id);
        /// <summary>
        /// Retrieve a train by it's unique id. This only works in network timetables.
        /// </summary>
        ITrain? GetTrainByQualifiedId(string id);
        /// <summary>
        /// This method removes the given Train from this timetable instance.
        /// </summary>
        void RemoveTrain(ITrain tra);

        /// <summary>
        /// Speicifies whether this train contains cyclic routes.
        /// </summary>
        bool HasRouteCycles { get; }
        /// <summary>
        /// This function adds an additional new route to an already added station.
        /// </summary>
        void StationAddRoute(Station sta, int route);
        /// <summary>
        /// This function removes a route from the given station.
        /// </summary>
        void StationRemoveRoute(Station sta, int route);
        /// <summary>
        /// Adds a new route between <paramref name="exisitingStartStation"/> and <paramref name="newStation"/>. <paramref name="newStation"/> has to be a new station not added to the timetable yet.
        /// It will create a new route containing only <paramref name="exisitingStartStation"/> @position <paramref name="newStartPosition"/> and <paramref name="newStation"/> @position <paramref name="newPosition"/>
        /// It will also add <paramref name="newStation"/> to the timetable.
        /// </summary>
        /// <param name="exisitingStartStation"></param>
        /// <param name="newStation">This station has to be "plain", e.g. have no routes &amp; no associated positions.</param>
        /// <param name="newStartPosition"></param>
        /// <param name="newPosition"></param>
        /// <returns>The index of the newly added route.</returns>
        /// <remarks>This operation is only supported on network timetables.</remarks>
        int AddRoute(Station exisitingStartStation, Station newStation, float newStartPosition, float newPosition);
        /// <summary>
        /// Connects an alredy existing station with another route to create circular networks.
        /// </summary>
        /// <param name="route">The pre-existing route to be added to the stations.</param>
        /// <param name="station"></param>
        /// <param name="newKm">The new position of <paramref name="station"/> on <paramref name="route"/>.</param>
        /// <returns>Specifies whether the operation has been successful.</returns>
        bool JoinRoutes(int route, Station station, float newKm);
        /// <summary>
        /// Gets the route with the specifiec route index.
        /// </summary>
        Route GetRoute(int index);
        /// <summary>
        /// Get all defined routes from this timetable instance.
        /// </summary>
        Route[] GetRoutes();
        /// <summary>
        /// Checks, whether <paramref name="routeToCheck"/> connects both given stations without any stations in between.
        /// </summary>
        bool RouteConnectsDirectly(int routeToCheck, Station sta1, Station sta2);
        /// <summary>
        /// Get the first route (index) that directly connects both stations, with no other station in between.
        /// </summary>
        int GetDirectlyConnectingRoute(Station sta1, Station sta2);
        
        /// <summary>
        /// Decides, if, after removal of <paramref name="toDelete"/>, an ambiguous route situation would occur, that
        /// means, there will be more than one route directly connection two other stations.
        /// </summary>
        /// <remarks>
        /// An ambiguous route are two or more routes that "collapsed" to one between at least two stations and thus are
        /// not distinguishable by the user or the application.
        /// </remarks>
        /// <seealso cref="RouteConnectsDirectly"/>
        bool WouldProduceAmbiguousRoute(Station toDelete);

        /// <summary>
        /// Creates or sets the transaition between the two trains: From <paramref name="first"/> to each <paramref name="trans"/>.
        /// </summary>
        void SetTransitions(ITrain first, IEnumerable<TransitionEntry> trans);
        /// <summary>
        /// Get the next train, following a single transition, if one exists.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="daysFilter">Returns only transitions that are valid on the given days.</param>
        /// <param name="stationFilter">Returns only transitions that are valid at the given station.</param>
        /// <returns>The next train or null, if none exists.</returns>
        /// <remarks>
        /// <para>On <see cref="Version"/>s lower than <see cref="TimetableVersion.JTG3_2"/>, all filters given with
        /// <paramref name="daysFilter"/> and <paramref name="stationFilter"/> will be silently ignored!</para>
        /// <para><paramref name="daysFilter"/> does not filter on the returned trains <see cref="ITrain.Days"/>!</para>
        /// </remarks>
        ITrain? GetTransition(ITrain? first, Days? daysFilter = null, Station? stationFilter = null);
        /// <summary>
        /// Removes the transition starting with this train.
        /// </summary>
        /// <param name="first">The train to search.</param>
        /// <param name="onlyAsFirst">If false, also remove all transitions leading to this train, thus removing all transitions referencing this train.</param>
        void RemoveTransition(ITrain first, bool onlyAsFirst = true);
        /// <summary>
        /// Removes the transition starting with this train.
        /// </summary>
        /// <param name="firstQualifiedTrainId">The qualified id of the train to search.</param>
        /// <param name="onlyAsFirst">If false, also remove all transitions leading to this train, thus removing all transitions referencing this train.</param>
        void RemoveTransition(string firstQualifiedTrainId, bool onlyAsFirst = true);
        /// <summary>
        /// Returns, if a transition with the given train as first train exists.
        /// </summary>
        /// <param name="tra">The train to search.</param>
        /// <param name="onlyAsFirst">If false, also check all transitions leading to this train, thus returning if any transition references this train..</param>
        /// <returns></returns>
        bool HasTransition(ITrain tra, bool onlyAsFirst = true);

        /// <summary>
        /// Create a deep copy (including XML-tree) of this timetable instance.
        /// </summary>
        Timetable Clone();

        /// <summary>
        /// Remove the specified link and all linked trains from this timetable. 
        /// </summary>
        void RemoveLink(TrainLink link);
        
        // Legacy linear APIs
        /// <summary>
        /// Returns all linear stations ordered by the given train direction. This does not work on network timetables obviosly.
        /// </summary>
        /// <remarks><see cref="TrainDirection.tr"/> is not a valid sort direction as it is not allowed on linear timetables.</remarks>
        List<Station> GetLinearStationsOrderedByDirection(TrainDirection direction);
        /// <summary>
        /// Returns a display name based on the linear order of the stations. This does not work on network timetables obviosly.
        /// </summary>
        /// <remarks><see cref="TrainDirection.tr"/> is not a valid sort direction as it is not allowed on linear timetables.</remarks>
        string GetLinearLineName(TrainDirection direction);
    }
}
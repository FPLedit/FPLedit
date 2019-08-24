using System.Collections.Generic;

namespace FPLedit.Shared
{
    public interface ITrain : IEntity
    {
        int Id { get; set; }
        bool IsLink { get; set; }
        string Comment { get; set; }
        Days Days { get; set; }
        TrainDirection Direction { get; }
        string Last { get; set; }
        string Locomotive { get; set; }
        string Mbr { get; set; }
        string TName { get; set; }

        string GetLineName();

        void AddAllArrDeps(IEnumerable<Station> path);
        List<Station> GetPath();
        ArrDep AddArrDep(Station sta, int route);
        ArrDep GetArrDep(Station sta);
        bool TryGetArrDep(Station sta, out ArrDep arrDep);
        Dictionary<Station, ArrDep> GetArrDeps();
        void RemoveArrDep(Station sta);
        void RemoveOrphanedTimes();
    }
}
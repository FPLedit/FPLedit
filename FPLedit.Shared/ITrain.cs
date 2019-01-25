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

        void AddAllArrDeps(List<Station> path);
        List<Station> GetPath();
        void AddArrDep(Station sta, ArrDep ardp, int route);
        void SetArrDep(Station sta, ArrDep ardp);
        ArrDep GetArrDep(Station sta);
        Dictionary<Station, ArrDep> GetArrDeps();
        void RemoveArrDep(Station sta);
        void RemoveOrphanedTimes();
    }
}
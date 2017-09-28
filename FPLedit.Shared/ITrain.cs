namespace FPLedit.Shared
{
    public interface ITrain : IEntity
    {
        string Comment { get; set; }
        bool[] Days { get; set; }
        TrainDirection Direction { get; }
        string Last { get; set; }
        string Locomotive { get; set; }
        string Mbr { get; set; }
        string TName { get; set; }

        void AddArrDep(Station sta, ArrDep ardp);
        ArrDep GetArrDep(Station sta);
        void RemoveArrDep(Station sta);
        void RemoveOrphanedTimes();
        void SetArrDep(Station sta, ArrDep ardp);
    }
}
namespace FPLedit.Shared
{
    public interface IPatternProvider
    {
        string TrainPatterns { get; set; }

        string StationPatterns { get; set; }
    }
}
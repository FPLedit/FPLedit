namespace FPLedit.Shared;

public interface IPatternSource
{
    string TrainPatterns { get; set; }

    string StationPatterns { get; set; }
}
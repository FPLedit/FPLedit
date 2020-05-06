namespace FPLedit.Shared
{
    public interface ISortedStations
    {
        Station[] GetSurroundingStations(Station center, int radius);
    }
}
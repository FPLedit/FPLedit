using FPLedit.Shared;

namespace FPLedit.Bildfahrplan
{
    public static class TimeEntryExtensions
    {
        public static string ToString(this Station sta, bool kilometre, int route)
            => sta.SName + (kilometre ? (" (" + sta.Positions.GetPosition(route) + ")") : "");
    }
}

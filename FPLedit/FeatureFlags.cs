using FPLedit.Shared;

namespace FPLedit
{
    public static class FeatureFlags
    {
        public static bool AdditionalStationAttributes { get; private set; } = false;

        public static void Initialize(IReadOnlySettings settings)
        {
            AdditionalStationAttributes = settings.Get<bool>("feature.station-additional-attributes", false);
        }
    }
}
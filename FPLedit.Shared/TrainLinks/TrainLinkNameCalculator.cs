using System;
using System.Collections.Generic;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public static class TrainLinkNameCalculatorManager
    {
        private static readonly Dictionary<string, Type> registered = new Dictionary<string, Type>()
        {
            [AutoTrainNameCalculator.PREFIX] = typeof(AutoTrainNameCalculator),
            [SpecialTrainNameCalculator.PREFIX] = typeof(SpecialTrainNameCalculator),
        };

        public static ITrainLinkNameCalculator Deserialize(string trainNamingScheme)
        {
            //HACK: This is intentional and may lead to a crash when a part contains a semicolon. But jTrainGraph does is like that.
            var parts = trainNamingScheme.Split(';');
            if (parts.Length < 2)
                throw new FormatException("Train link naming scheme is to short: " + trainNamingScheme);
            
            if (registered.TryGetValue(parts[0], out var tnct) && typeof(ITrainLinkNameCalculator).IsAssignableFrom(tnct))
            {
                var tnc = (ITrainLinkNameCalculator) Activator.CreateInstance(tnct);
                tnc.Deserialize(parts);
                return tnc;
            }

            return null;
        }

        //HACK: This is intentional and may lead to a crash when a part contains a semicolon. But jTrainGraph does is like that.
        public static string Serialize(ITrainLinkNameCalculator tnc) => string.Join(";", tnc.Serialize());
    }
}
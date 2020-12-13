using System;
using System.Collections.Generic;

namespace FPLedit.Shared.TrainLinks
{
    /// <summary>
    /// Helper class for using <see cref="ITrainNameGen"/>s.
    /// </summary>
    [Templating.TemplateSafe]
    public static class TrainNameGenManager
    {
        private static readonly Dictionary<string, Type> registered = new Dictionary<string, Type>()
        {
            [AutoTrainNameGen.PREFIX] = typeof(AutoTrainNameGen),
            [SpecialTrainNameGen.PREFIX] = typeof(SpecialTrainNameGen),
        };

        /// <summary>
        /// Initialize an empty instance from a serialized string as it will be stored in the XML structure.
        /// </summary>
        public static ITrainNameGen Deserialize(string trainNamingScheme)
        {
            //HACK: This is intentional and may lead to a crash when a part contains a semicolon. But jTrainGraph does is like that.
            var parts = trainNamingScheme.Split(';');
            if (parts.Length < 2)
                throw new FormatException("Train link naming scheme is to short: " + trainNamingScheme);
            
            if (registered.TryGetValue(parts[0], out var tnct) && typeof(ITrainNameGen).IsAssignableFrom(tnct))
            {
                var tnc = (ITrainNameGen?) Activator.CreateInstance(tnct);
                tnc?.Deserialize(parts);
                return tnc ?? throw new FormatException("Train link nameing scheme " + parts[0] + " could not be instandiated!");
            }

            throw new FormatException("Train link nameing scheme " + parts[0] + " not found!");
        }

        //HACK: This is intentional and may lead to a crash when a part contains a semicolon. But jTrainGraph does is like that.
        /// <summary>
        /// Serialize this instance to a string as it will be stored in the XML structure.
        /// </summary>
        public static string Serialize(ITrainNameGen tnc) => string.Join(";", tnc.Serialize());
    }
}
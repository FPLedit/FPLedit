using System.Collections.Generic;

namespace FPLedit.Shared.TrainLinks
{
    /// <summary>
    /// Interface for different train naming schemes for linked trains.
    /// </summary>
    [Templating.TemplateSafe]
    public interface ITrainNameGen
    {
        /// <summary>
        /// Initialize an empty instance from a serialized string as it will be stored in the XML structure.
        /// </summary>
        void Deserialize(IEnumerable<string> parts);
        /// <summary>
        /// Serialize this instance to a string as it will be stored in the XML structure.
        /// </summary>
        IEnumerable<string> Serialize();
        /// <summary>
        /// Get a train name for the specified train with the giving counting index.
        /// </summary>
        /// <param name="countingIndex">The zero-based counting index of the linked train, relative to the parent link.</param>
        string GetTrainName(int countingIndex);
    }
}
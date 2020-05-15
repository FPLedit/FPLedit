using System.Collections.Generic;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public interface ITrainLinkNameCalculator
    {
        void Deserialize(IEnumerable<string> parts);
        IEnumerable<string> Serialize();
        string GetTrainName(int countingIndex);
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface IPreviewProxy
    {
        string DisplayName { get; }

        void Show(IPluginInterface pluginInterface);
    }
}

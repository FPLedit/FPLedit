using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface IPreviewAction
    {
        string DisplayName { get; }
        
        string MenuName { get; }

        void Show(IPluginInterface pluginInterface);
    }
}

using System;
using System.Linq;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;

namespace FPLedit.CorePlugins
{
    internal sealed class MenuPlugin : IPlugin
    {
        private IPluginInterface pluginInterface = null!;
        private ButtonMenuItem editRoot = null!, previewRoot = null!;
        private int dialogOffset;
        private IEditMenuItemAction[] editMenuActions = Array.Empty<IEditMenuItemAction>();

        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            this.pluginInterface = pluginInterface;
            pluginInterface.ExtensionsLoaded += PluginInterface_ExtensionsLoaded;
            pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;

            editRoot = (ButtonMenuItem) ((MenuBar) pluginInterface.Menu).GetItem(MainForm.LocEditMenu)!;
            previewRoot = (ButtonMenuItem) ((MenuBar) pluginInterface.Menu).GetItem(MainForm.LocPreviewMenu)!;
        }
        
        private void PluginInterface_ExtensionsLoaded(object? sender, EventArgs e)
        {
            var previewables = pluginInterface.GetRegistered<IPreviewAction>();
            if (previewables.Length == 0)
                pluginInterface.Menu.Items.Remove(previewRoot); // Ausblenden in der harten Art

#pragma warning disable CA2000
            foreach (var prev in previewables)
                previewRoot.CreateItem(prev.MenuName, enabled: false, clickHandler: (_, _) => prev.Show(pluginInterface));
#pragma warning restore CA2000

            editMenuActions = pluginInterface.GetRegistered<IEditMenuItemAction>();
            if (editMenuActions.Length > 0)
                editRoot.Items.Add(new SeparatorMenuItem());

            dialogOffset = editRoot.Items.Count;
#pragma warning disable CA2000
            foreach (var dialog in editMenuActions)
                editRoot.CreateItem(dialog.DisplayName, enabled: dialog.IsEnabled(pluginInterface), clickHandler: (_, _) => dialog.Invoke(pluginInterface));
#pragma warning restore CA2000
        }
        
        private void PluginInterface_FileStateChanged(object? sender, FileStateChangedEventArgs e)
        {
            foreach (var ddi in previewRoot.Items.OfType<ButtonMenuItem>())
                ddi.Enabled = e.FileState.Opened && e.FileState.LineCreated;

            for (int i = 0; i < editMenuActions.Length; i++)
            {
                var elem = editRoot.Items[dialogOffset + i];
                elem.Enabled = editMenuActions[i].IsEnabled(pluginInterface);
            }
        }
    }
}
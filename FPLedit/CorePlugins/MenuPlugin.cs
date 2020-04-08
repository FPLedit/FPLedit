using System;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;

namespace FPLedit.CorePlugins
{
    internal sealed class MenuPlugin : IPlugin
    {
        private IPluginInterface pluginInterface;
        private ButtonMenuItem editRoot, previewRoot;
        private int dialogOffset;
        private IEditMenuItemAction[] editMenuActions;

        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            this.pluginInterface = pluginInterface;
            pluginInterface.ExtensionsLoaded += PluginInterface_ExtensionsLoaded;
            pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;

            editRoot = (ButtonMenuItem) ((MenuBar) pluginInterface.Menu).GetItem(MainForm.LocEditMenu);
            previewRoot = (ButtonMenuItem) ((MenuBar) pluginInterface.Menu).GetItem(MainForm.LocPreviewMenu);
        }
        
        private void PluginInterface_ExtensionsLoaded(object sender, EventArgs e)
        {
            var previewables = pluginInterface.GetRegistered<IPreviewAction>();
            if (previewables.Length == 0)
                pluginInterface.Menu.Items.Remove(previewRoot); // Ausblenden in der harten Art

            foreach (var prev in previewables)
                previewRoot.CreateItem(prev.MenuName, enabled: false, clickHandler: (s, ev) => prev.Show(pluginInterface));

            editMenuActions = pluginInterface.GetRegistered<IEditMenuItemAction>();
            if (editMenuActions.Length > 0)
                editRoot.Items.Add(new SeparatorMenuItem());

            dialogOffset = editRoot.Items.Count;
            foreach (var dialog in editMenuActions)
                editRoot.CreateItem(dialog.DisplayName, enabled: dialog.IsEnabled(pluginInterface), clickHandler: (s, ev) => dialog.Invoke(pluginInterface));
        }
        
        private void PluginInterface_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            foreach (ButtonMenuItem ddi in previewRoot.Items)
                ddi.Enabled = e.FileState.Opened && e.FileState.LineCreated;

            for (int i = 0; i < editMenuActions.Length; i++)
            {
                var elem = editRoot.Items[dialogOffset + i];
                elem.Enabled = editMenuActions[i].IsEnabled(pluginInterface);
            }
        }
    }
}
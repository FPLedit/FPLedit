#nullable enable
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;

namespace FPLedit.Editor
{
    internal sealed class EditorPlugin : IPlugin
    {
        private IPluginInterface pluginInterface = null!;

        private bool hasFilterables, hasDesignables, hasVirtualRouteSupport;

        private ButtonMenuItem editLineItem = null!, editTimetableItem = null!, virtualRoutesItem = null!; // Type="Network"
        private ButtonMenuItem editTrainsItem = null!, designItem = null!, filterItem = null!; // Type="Both"
        private ButtonMenuItem editRoot = null!, undoItem = null!; // Type="Both"

        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            this.pluginInterface = pluginInterface;
            pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;
            pluginInterface.ExtensionsLoaded += PluginInterface_ExtensionsLoaded;
            
            if (Environment.OSVersion.Platform != PlatformID.Win32NT || pluginInterface.Settings.Get<bool>("mp-compat.route-edit-button"))
                componentRegistry.Register<IRouteAction>(new Network.EditRouteAction());

            editRoot = (ButtonMenuItem)((MenuBar)pluginInterface.Menu).GetItem(MainForm.LocEditMenu)!;

            undoItem = editRoot.CreateItem(T._("&Rückgängig"), enabled: false, shortcut: Keys.Control | Keys.Z, clickHandler: (_, _) => pluginInterface.Undo());

            editRoot.Items.Add(new SeparatorMenuItem());

            editLineItem = editRoot.CreateItem(T._("&Strecke bearbeiten (linear)"), enabled: false, shortcut: Keys.Control | Keys.L,
                clickHandler: (_, _) => ShowForm(new LineEditForm(pluginInterface, Timetable.LINEAR_ROUTE_ID)));

            editTrainsItem = editRoot.CreateItem(T._("&Züge bearbeiten"), enabled: false, shortcut: Keys.Control | Keys.R);
            editTrainsItem.Click += (_, _) =>
            {
                if (pluginInterface.Timetable.Type == TimetableType.Linear)
                    ShowForm(new Linear.LinearTrainsEditForm(pluginInterface));
                else ShowForm(new Network.NetworkTrainsEditForm(pluginInterface));
            };

            editTimetableItem = editRoot.CreateItem(T._("&Fahrplan bearbeiten"), enabled: false, shortcut: Keys.Control | Keys.T);
            editTimetableItem.Click += (_, _) =>
            {
                if (pluginInterface.Timetable.Type == TimetableType.Linear)
                    ShowForm(new Linear.LinearTimetableEditForm(pluginInterface));
                else ShowForm(new Network.MultipleTimetableEditForm(pluginInterface));
            };

            editRoot.Items.Add(new SeparatorMenuItem());

            designItem = editRoot.CreateItem(T._("Fahrplan&darstellung"), enabled: false, clickHandler: (_, _) => ShowForm(new RenderSettingsForm(pluginInterface)));
            filterItem = editRoot.CreateItem(T._("Fi&lterregeln"), enabled: false, clickHandler: (_, _) => ShowForm(new Filters.FilterForm(pluginInterface)));

            virtualRoutesItem = editRoot.CreateItem(T._("&Virtuelle Strecken"), enabled: false, clickHandler: (_, _) => ShowForm(new Network.VirtualRouteForm(pluginInterface)));
        }

        private void ShowForm(Dialog<DialogResult> form)
        {
            pluginInterface.StageUndoStep();
            if (form.ShowModal(Program.App!.MainForm) == DialogResult.Ok)
                pluginInterface.SetUnsaved();
            if (!form.IsDisposed)
                form.Dispose();
        }

        private void PluginInterface_ExtensionsLoaded(object? sender, EventArgs e)
        {
            hasFilterables = pluginInterface.GetRegistered<IFilterRuleContainer>().Length > 0;
            hasDesignables = pluginInterface.GetRegistered<IAppearanceControl>().Length > 0;
            hasVirtualRouteSupport = pluginInterface.GetRegistered<ISupportsVirtualRoutes>().Length > 0;
        }

        private void PluginInterface_FileStateChanged(object? sender, FileStateChangedEventArgs e)
        {
            editLineItem.Enabled = e.FileState.Opened;
            editTrainsItem.Enabled = virtualRoutesItem.Enabled = e.FileState.Opened && e.FileState.LineCreated;
            editTimetableItem.Enabled = e.FileState.Opened && e.FileState.LineCreated && e.FileState.TrainsCreated;
            designItem.Enabled = e.FileState.Opened && hasDesignables;
            filterItem.Enabled = e.FileState.Opened && hasFilterables;
            undoItem.Enabled = e.FileState.CanGoBack;

            // Im Netzwerk-Modus nicht verwendete Menü-Einträge ausblenden
            if (pluginInterface.TimetableMaybeNull != null!)
                editLineItem.Enabled = pluginInterface.Timetable.Type != TimetableType.Network;
            virtualRoutesItem.Visible = hasVirtualRouteSupport && pluginInterface.TimetableMaybeNull?.Type == TimetableType.Network;
        }
    }
}

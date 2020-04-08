using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Editor
{
    internal sealed class EditorPlugin : IPlugin
    {
        private IPluginInterface pluginInterface;

        private bool hasFilterables, hasDesignables;

        private ButtonMenuItem editLineItem, editTimetableItem; // Type="Network"
        private ButtonMenuItem editTrainsItem, designItem, filterItem; // Type="Both"
        private ButtonMenuItem editRoot, undoItem; // Type="Both"

        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            this.pluginInterface = pluginInterface;
            pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;
            pluginInterface.ExtensionsLoaded += PluginInterface_ExtensionsLoaded;
            
            if (Environment.OSVersion.Platform != PlatformID.Win32NT || pluginInterface.Settings.Get<bool>("mp-compat.route-edit-button"))
                componentRegistry.Register<IRouteAction>(new Network.EditRouteAction());

            editRoot = (ButtonMenuItem)((MenuBar)pluginInterface.Menu).GetItem(MainForm.LocEditMenu);

            undoItem = editRoot.CreateItem("&Rückgängig", enabled: false, shortcut: Keys.Control | Keys.Z, clickHandler: (s, e) => pluginInterface.Undo());

            editRoot.Items.Add(new SeparatorMenuItem());

            editLineItem = editRoot.CreateItem("&Strecke bearbeiten (linear)", enabled: false, shortcut: Keys.Control | Keys.L,
                clickHandler: (s, e) => ShowForm(new LineEditForm(pluginInterface, Timetable.LINEAR_ROUTE_ID)));

            editTrainsItem = editRoot.CreateItem("&Züge bearbeiten", enabled: false, shortcut: Keys.Control | Keys.R);
            editTrainsItem.Click += (s, e) =>
            {
                if (pluginInterface.Timetable.Type == TimetableType.Linear)
                    ShowForm(new Linear.LinearTrainsEditForm(pluginInterface));
                else ShowForm(new Network.NetworkTrainsEditForm(pluginInterface));
            };

            editTimetableItem = editRoot.CreateItem("&Fahrplan bearbeiten", enabled: false , shortcut: Keys.Control | Keys.T);
            editTimetableItem.Click += (s, e) =>
            {
                if (pluginInterface.Timetable.Type == TimetableType.Linear)
                    ShowForm(new Linear.LinearTimetableEditForm(pluginInterface));
                else ShowForm(new Network.MultipleTimetableEditForm(pluginInterface));
            };

            editRoot.Items.Add(new SeparatorMenuItem());

            designItem = editRoot.CreateItem("Fahrplan&darstellung", enabled: false, clickHandler: (s, e) => ShowForm(new RenderSettingsForm(pluginInterface)));
            filterItem = editRoot.CreateItem("Fi&lterregeln", enabled: false, clickHandler: (s, e) => ShowForm(new Filters.FilterForm(pluginInterface)));
        }

        private void ShowForm(Dialog<DialogResult> form)
        {
            pluginInterface.StageUndoStep();
            if (form.ShowModal(Program.App.MainForm) == DialogResult.Ok)
                pluginInterface.SetUnsaved();
            if (!form.IsDisposed)
                form.Dispose();
        }

        private void PluginInterface_ExtensionsLoaded(object sender, EventArgs e)
        {
            hasFilterables = pluginInterface.GetRegistered<IFilterRuleContainer>().Length > 0;
            hasDesignables = pluginInterface.GetRegistered<IAppearanceControl>().Length > 0;
        }

        private void PluginInterface_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            editLineItem.Enabled = e.FileState.Opened;
            editTrainsItem.Enabled = e.FileState.Opened && e.FileState.LineCreated;
            editTimetableItem.Enabled = e.FileState.Opened && e.FileState.LineCreated && e.FileState.TrainsCreated;
            designItem.Enabled = e.FileState.Opened && hasDesignables;
            filterItem.Enabled = e.FileState.Opened && hasFilterables;
            undoItem.Enabled = e.FileState.CanGoBack;

            // Im Netzwerk-Modus nicht verwendete Menü-Einträge ausblenden
            if (pluginInterface.Timetable != null)
                editLineItem.Enabled = pluginInterface.Timetable.Type != TimetableType.Network;
        }
    }
}

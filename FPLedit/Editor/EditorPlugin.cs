using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Editor
{
    public class EditorPlugin : IPlugin
    {
        private IInfo info;

        private int dialogOffset;
        private IEditingDialog[] dialogs;
        private bool hasFilterables, hasDesignables;

        private ButtonMenuItem editLineItem, editTimetableItem; // Type="Network"
        private ButtonMenuItem editTrainsItem, designItem, filterItem; // Type="Both"
        private ButtonMenuItem editRoot, previewRoot, undoItem; // Type="Both"

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;
            info.ExtensionsLoaded += Info_ExtensionsLoaded;

            info.Register<IExport>(new FPLedit.NonDefaultFiletypes.CleanedXMLExport());
            info.Register<ITimetableCheck>(new FPLedit.TimetableChecks.TransitionsCheck());
            info.Register<ITimetableCheck>(new FPLedit.TimetableChecks.DayOverflowCheck());
            info.Register<ITimetableInitAction>(new FPLedit.TimetableChecks.UpdateColorsAction());
            info.Register<ITimetableInitAction>(new FPLedit.TimetableChecks.FixNetworkAttributesAction());

            if (Environment.OSVersion.Platform != PlatformID.Win32NT || info.Settings.Get<bool>("mp-compat.route-edit-button"))
                info.Register<IRouteAction>(new Network.EditRouteAction());

            editRoot = ((MenuBar)info.Menu).CreateItem("Bearbeiten");

            undoItem = editRoot.CreateItem("Rückgängig", enabled: false, clickHandler: (s, e) => info.Undo());
            undoItem.Shortcut = Keys.Control | Keys.Z;

            editRoot.Items.Add(new SeparatorMenuItem());

            editLineItem = editRoot.CreateItem("Strecke bearbeiten (linear)", enabled: false,
                clickHandler: (s, e) => ShowForm(new LineEditForm(info, Timetable.LINEAR_ROUTE_ID)));

            editTrainsItem = editRoot.CreateItem("Züge bearbeiten", enabled: false);
            editTrainsItem.Click += (s, e) =>
            {
                if (info.Timetable.Type == TimetableType.Linear)
                    ShowForm(new Linear.LinearTrainsEditForm(info));
                else ShowForm(new Network.NetworkTrainsEditForm(info));
            };

            editTimetableItem = editRoot.CreateItem("Fahrplan bearbeiten", enabled: false);
            editTimetableItem.Click += (s, e) =>
            {
                if (info.Timetable.Type == TimetableType.Linear)
                    ShowForm(new Linear.LinearTimetableEditForm(info));
                else ShowForm(new Network.MultipleTimetableEditForm(info));
            };

            editRoot.Items.Add(new SeparatorMenuItem());

            designItem = editRoot.CreateItem("Fahrplandarstellung", enabled: false, clickHandler: (s, e) => ShowForm(new RenderSettingsForm(info)));
            filterItem = editRoot.CreateItem("Filterregeln", enabled: false, clickHandler: (s, e) => ShowForm(new Filters.FilterForm(info)));

            previewRoot = ((MenuBar)info.Menu).CreateItem("Vorschau");
        }

        private void ShowForm(Dialog<DialogResult> form)
        {
            info.StageUndoStep();
            if (form.ShowModal(Program.App.MainForm) == DialogResult.Ok)
                info.SetUnsaved();
            form.Dispose();
        }

        private void Info_ExtensionsLoaded(object sender, EventArgs e)
        {
            var previewables = info.GetRegistered<IPreviewable>();
            if (previewables.Length == 0)
                info.Menu.Items.Remove(previewRoot); // Ausblenden in der harten Art

            foreach (var prev in previewables)
                previewRoot.CreateItem(prev.DisplayName, enabled: false, clickHandler: (s, ev) => prev.Show(info));

            dialogs = info.GetRegistered<IEditingDialog>();
            if (dialogs.Length > 0)
                editRoot.Items.Add(new SeparatorMenuItem());

            dialogOffset = editRoot.Items.Count;
            foreach (var dialog in dialogs)
                editRoot.CreateItem(dialog.DisplayName, enabled: dialog.IsEnabled(info), clickHandler: (s, ev) => dialog.Show(info));

            hasFilterables = info.GetRegistered<IFilterableUi>().Length > 0;
            hasDesignables = info.GetRegistered<IDesignableUiProxy>().Length > 0;
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            editLineItem.Enabled = e.FileState.Opened;
            editTrainsItem.Enabled = e.FileState.Opened && e.FileState.LineCreated;
            editTimetableItem.Enabled = e.FileState.Opened && e.FileState.LineCreated && e.FileState.TrainsCreated;
            designItem.Enabled = e.FileState.Opened && hasDesignables;
            filterItem.Enabled = e.FileState.Opened && hasFilterables;
            undoItem.Enabled = e.FileState.CanGoBack;

            foreach (ButtonMenuItem ddi in previewRoot.Items)
                ddi.Enabled = e.FileState.Opened && e.FileState.LineCreated;

            for (int i = 0; i < dialogs.Length; i++)
            {
                var elem = editRoot.Items[dialogOffset + i];
                elem.Enabled = dialogs[i].IsEnabled(info);
            }

            // Im Netzwerk-Modus nicht verwendete Menü-Einträge ausblenden
            if (info.Timetable != null)
                editLineItem.Enabled = info.Timetable.Type != TimetableType.Network;
        }
    }
}

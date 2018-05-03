using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
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

            info.Register<IExport>(new FPLedit.Shared.Filetypes.CleanedXMLExport());

            if (Environment.OSVersion.Platform != PlatformID.Win32NT || info.Settings.Get<bool>("mp-compat.route-edit-button"))
                info.Register<IRouteAction>(new Network.EditRouteAction());

            editRoot = MenuItem("Bearbeiten");
            info.Menu.Items.Add(editRoot);

            undoItem = MenuItem("Rückgängig", editRoot);
            undoItem.Shortcut = Keys.Control | Keys.Z;
            undoItem.Enabled = false;
            undoItem.Click += (s, e) => info.Undo();

            editRoot.Items.Add(new SeparatorMenuItem());

            editLineItem = MenuItem("Strecke bearbeiten (tabellarisch)", editRoot);
            editLineItem.Enabled = false;
            editLineItem.Click += (s, e) => ShowForm(new LineEditForm(info, Timetable.LINEAR_ROUTE_ID));

            editTrainsItem = MenuItem("Züge bearbeiten", editRoot);
            editTrainsItem.Enabled = false;
            editTrainsItem.Click += (s, e) =>
            {
                if (info.Timetable.Type == TimetableType.Linear)
                    ShowForm(new Linear.TrainsEditForm(info));
                else ShowForm(new Network.TrainsEditingForm(info));
            };

            editTimetableItem = MenuItem("Fahrplan bearbeiten", editRoot);
            editTimetableItem.Enabled = false;
            editTimetableItem.Click += (s, e) => ShowForm(new Linear.TimetableEditForm(info));

            editRoot.Items.Add(new SeparatorMenuItem());

            designItem = MenuItem("Fahrplandarstellung", editRoot);
            designItem.Enabled = false;
            designItem.Click += (s, e) => ShowForm(new DesignableForm(info));

            filterItem = MenuItem("Filterregeln", editRoot);
            filterItem.Enabled = false;
            filterItem.Click += (s, e) => ShowForm(new Filters.FilterForm(info));

            previewRoot = MenuItem("Vorschau");
            info.Menu.Items.Add(previewRoot);
        }

        private void ShowForm(Dialog<DialogResult> form)
        {
            info.StageUndoStep();
            if (form.ShowModal(Program.App.MainForm) == DialogResult.Ok)
                info.SetUnsaved();
        }

        private void Info_ExtensionsLoaded(object sender, EventArgs e)
        {
            var previewables = info.GetRegistered<IPreviewable>();
            if (previewables.Length == 0)
                info.Menu.Items.Remove(previewRoot); // Ausblenden in der harten Art

            foreach (var prev in previewables)
            {
                var itm = MenuItem(prev.DisplayName, previewRoot);
                itm.Enabled = false;
                itm.Click += (s, ev) => prev.Show(info);
            }

            dialogs = info.GetRegistered<IEditingDialog>();
            if (dialogs.Length > 0)
                editRoot.Items.Add(new SeparatorMenuItem());

            dialogOffset = editRoot.Items.Count;
            foreach (var dialog in dialogs)
            {
                var itm = MenuItem(dialog.DisplayName, editRoot);
                itm.Enabled = dialog.IsEnabled(info);
                itm.Click += (s, ev) => dialog.Show(info);
            }

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
            {
                //TODO: reimplemnt Visible
                editLineItem.Enabled = editTimetableItem.Enabled = info.Timetable.Type != TimetableType.Network;
            }
        }

        private ButtonMenuItem MenuItem(string text)
        {
            var itm = new ButtonMenuItem();
            itm.Text = text;
            return itm;
        }

        private ButtonMenuItem MenuItem(string text, ButtonMenuItem parent)
        {
            var itm = new ButtonMenuItem();
            itm.Text = text;
            parent.Items.Add(itm);
            return itm;
        }
    }
}

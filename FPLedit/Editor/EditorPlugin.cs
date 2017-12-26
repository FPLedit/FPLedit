using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Editor
{
    public class EditorPlugin : IPlugin
    {
        private IInfo info;

        private int dialogOffset;
        private IEditingDialog[] dialogs;
        private bool hasFilterables, hasDesignables;

        private ToolStripItem editLineItem, editTimetableItem; // Type="Network"
        private ToolStripItem editTrainsItem, designItem, filterItem; // Type="Both"
        private ToolStripMenuItem editRoot, previewRoot, undoItem; // Type="Both"

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;
            info.ExtensionsLoaded += Info_ExtensionsLoaded;

            info.Register<IExport>(new FPLedit.Shared.Filetypes.CleanedXMLExport());

            editRoot = new ToolStripMenuItem("Bearbeiten");
            info.Menu.Items.Add(editRoot);

            undoItem = (ToolStripMenuItem)editRoot.DropDownItems.Add("Rückgängig");
            undoItem.ShortcutKeys = Keys.Control | Keys.Z;
            undoItem.Enabled = false;
            undoItem.Click += (s, e) => info.Undo();

            editRoot.DropDownItems.Add(new ToolStripSeparator());

            editLineItem = editRoot.DropDownItems.Add("Strecke bearbeiten (tabellarisch)");
            editLineItem.Enabled = false;
            editLineItem.Click += (s, e) => ShowForm(new Linear.LineEditForm(info));

            editTrainsItem = editRoot.DropDownItems.Add("Züge bearbeiten");
            editTrainsItem.Enabled = false;
            editTrainsItem.Click += (s, e) =>
            {
                if (info.Timetable.Type == TimetableType.Linear)
                    ShowForm(new Linear.TrainsEditForm(info));
                else ShowForm(new Network.TrainsEditingForm(info));
            };

            editTimetableItem = editRoot.DropDownItems.Add("Fahrplan bearbeiten");
            editTimetableItem.Enabled = false;
            editTimetableItem.Click += (s, e) => ShowForm(new Linear.TimetableEditForm(info));

            editRoot.DropDownItems.Add(new ToolStripSeparator());

            designItem = editRoot.DropDownItems.Add("Fahrplandarstellung");
            designItem.Enabled = false;
            designItem.Click += (s, e) => ShowForm(new DesignableForm(info));

            filterItem = editRoot.DropDownItems.Add("Filterregeln");
            filterItem.Enabled = false;
            filterItem.Click += (s, e) => ShowForm(new FilterForm(info));

            previewRoot = new ToolStripMenuItem("Vorschau");
            info.Menu.Items.Add(previewRoot);
        }

        private void ShowForm(Form form)
        {
            info.StageUndoStep();
            if (form.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        private void Info_ExtensionsLoaded(object sender, EventArgs e)
        {
            var previewables = info.GetRegistered<IPreviewable>();
            if (previewables.Length == 0)
                previewRoot.Visible = false;

            foreach (var prev in previewables)
            {
                var itm = previewRoot.DropDownItems.Add(prev.DisplayName);
                itm.Enabled = false;
                itm.Click += (s, ev) => prev.Show(info);
            }

            dialogs = info.GetRegistered<IEditingDialog>();
            if (dialogs.Length > 0)
                editRoot.DropDownItems.Add(new ToolStripSeparator());

            dialogOffset = editRoot.DropDownItems.Count;
            foreach (var dialog in dialogs)
            {
                var itm = editRoot.DropDownItems.Add(dialog.DisplayName);
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

            foreach (ToolStripItem ddi in previewRoot.DropDownItems)
                ddi.Enabled = e.FileState.Opened && e.FileState.LineCreated;

            for (int i = 0; i < dialogs.Length; i++)
            {
                var elem = editRoot.DropDownItems[dialogOffset + i];
                elem.Enabled = dialogs[i].IsEnabled(info);
            }

            // Im Netzwerk-Modus nicht verwendete Menü-Einträge ausblenden
            if (info.Timetable != null && info.Timetable.Type == TimetableType.Network)
            {
                editLineItem.Visible = false;
                editTimetableItem.Visible = false;
            }
        }
    }
}

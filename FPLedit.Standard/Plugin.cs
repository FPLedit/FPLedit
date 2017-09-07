using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Standard
{
    [Plugin("Fahrplan-Editoren", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ToolStripItem editLineItem, editTrainsItem, editTimetableItem, designItem, filterItem;
        private ToolStripMenuItem eitem, pitem;
        private int dialogOffset;

        private IEditingDialog[] dialogs;
        private bool hasFilterables, hasDesignables;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;
            info.ExtensionsLoaded += Info_ExtensionsLoaded;

            info.Register<IExport>(new Shared.Filetypes.CleanedXMLExport());

            eitem = new ToolStripMenuItem("Bearbeiten");
            info.Menu.Items.Add(eitem);

            editLineItem = eitem.DropDownItems.Add("Strecke bearbeiten");
            editLineItem.Enabled = false;
            editLineItem.Click += EditLineItem_Click;

            editTrainsItem = eitem.DropDownItems.Add("Züge bearbeiten");
            editTrainsItem.Enabled = false;
            editTrainsItem.Click += EditTrainsItem_Click;

            editTimetableItem = eitem.DropDownItems.Add("Fahrplan bearbeiten");
            editTimetableItem.Enabled = false;
            editTimetableItem.Click += EditTimetableItem_Click;

            eitem.DropDownItems.Add(new ToolStripSeparator());

            designItem = eitem.DropDownItems.Add("Fahrplandarstellung");
            designItem.Enabled = false;
            designItem.Click += DesignItem_Click;

            filterItem = eitem.DropDownItems.Add("Filterregeln");
            filterItem.Enabled = false;
            filterItem.Click += FilterItem_Click;

            pitem = new ToolStripMenuItem("Vorschau");
            info.Menu.Items.Add(pitem);
        }

        private void Info_ExtensionsLoaded(object sender, EventArgs e)
        {
            var previewables = info.GetRegistered<IPreviewable>();
            if (previewables.Length == 0)
                pitem.Visible = false;

            foreach (var prev in previewables)
            {
                var itm = pitem.DropDownItems.Add(prev.DisplayName);
                itm.Enabled = false;
                itm.Click += (s, ev) => prev.Show(info);
            }

            dialogs = info.GetRegistered<IEditingDialog>();
            if (dialogs.Length > 0)
                eitem.DropDownItems.Add(new ToolStripSeparator());

            dialogOffset = eitem.DropDownItems.Count;
            foreach (var dialog in dialogs)
            {
                var itm = eitem.DropDownItems.Add(dialog.DisplayName);
                itm.Enabled = dialog.IsEnabled(info);
                itm.Click += (s, ev) => dialog.Show(info);
            }

            hasFilterables = info.GetRegistered<IFilterableUi>().Length > 0;
            hasDesignables = info.GetRegistered<IDesignableUiProxy>().Length > 0;
        }

        private void FilterItem_Click(object sender, EventArgs e)
        {
            var ff = new FilterForm(info);
            if (ff.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        private void DesignItem_Click(object sender, EventArgs e)
        {
            var df = new DesignableForm(info);
            if (df.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        private void EditTimetableItem_Click(object sender, EventArgs e)
        {
            var ttEdit = new TimetableEditForm(info);
            if (ttEdit.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        private void EditTrainsItem_Click(object sender, EventArgs e)
        {
            var trEdit = new TrainsEditForm(info);
            if (trEdit.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        private void EditLineItem_Click(object sender, EventArgs e)
        {
            var liEdit = new LineEditForm(info);
            if (liEdit.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            editLineItem.Enabled = e.FileState.Opened;
            editTrainsItem.Enabled = e.FileState.Opened && e.FileState.LineCreated;
            editTimetableItem.Enabled = e.FileState.Opened && e.FileState.LineCreated && e.FileState.TrainsCreated;
            designItem.Enabled = e.FileState.Opened && hasDesignables;
            filterItem.Enabled = e.FileState.Opened && hasFilterables;

            foreach (ToolStripItem ddi in pitem.DropDownItems)
                ddi.Enabled = e.FileState.Opened && e.FileState.LineCreated;

            for (int i = 0; i < dialogs.Length; i++)
            {
                var elem = eitem.DropDownItems[dialogOffset + i];
                elem.Enabled = dialogs[i].IsEnabled(info);
            }
        }
    }
}

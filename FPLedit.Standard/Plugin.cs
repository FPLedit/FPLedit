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
        private ToolStripMenuItem pitem;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.Register<IExport>(new Shared.Filetypes.CleanedXMLExport());

            var item = new ToolStripMenuItem("Bearbeiten");
            info.Menu.Items.Add(item);

            editLineItem = item.DropDownItems.Add("Strecke bearbeiten");
            editLineItem.Enabled = false;
            editLineItem.Click += EditLineItem_Click;

            editTrainsItem = item.DropDownItems.Add("Züge bearbeiten");
            editTrainsItem.Enabled = false;
            editTrainsItem.Click += EditTrainsItem_Click;

            editTimetableItem = item.DropDownItems.Add("Fahrplan bearbeiten");
            editTimetableItem.Enabled = false;
            editTimetableItem.Click += EditTimetableItem_Click;

            item.DropDownItems.Add(new ToolStripSeparator());

            designItem = item.DropDownItems.Add("Fahrplandarstellung");
            designItem.Enabled = false;
            designItem.Click += DesignItem_Click;

            filterItem = item.DropDownItems.Add("Filterregeln");
            filterItem.Enabled = false;
            filterItem.Click += FilterItem_Click;

            pitem = new ToolStripMenuItem("Vorschau");
            info.Menu.Items.Add(pitem);
            pitem.Enabled = false;
        }

        private void FilterItem_Click(object sender, EventArgs e)
        {
            new FilterForm(info).ShowDialog();
        }

        private void DesignItem_Click(object sender, EventArgs e)
        {
            new DesignableForm(info).ShowDialog();
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

        bool loaded = false;
        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            editLineItem.Enabled = designItem.Enabled = filterItem.Enabled = e.FileState.Opened;
            editTrainsItem.Enabled = pitem.Enabled = e.FileState.Opened && e.FileState.LineCreated;
            editTimetableItem.Enabled = e.FileState.Opened && e.FileState.LineCreated && e.FileState.TrainsCreated;

            if (!loaded)
            {
                var previewables = info.GetRegistered<IPreviewable>();
                foreach (var prev in previewables)
                {
                    var itm = pitem.DropDownItems.Add(prev.DisplayName);
                    //itm.Enabled = false;
                    itm.Click += (s, ev) => prev.Show(info);
                }
            }
            loaded = true;
        }
    }
}

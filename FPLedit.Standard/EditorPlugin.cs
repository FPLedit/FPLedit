using FPLedit.Shared;
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
    public class EditorPlugin : IPlugin
    {
        public string Name
        {
            get { return "Fahrplan-Editoren"; }
        }

        private IInfo info;
        private ToolStripItem editLineItem, editTrainsItem, editTimetableItem;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;            

            ToolStripMenuItem item = new ToolStripMenuItem("Bearbeiten");
            info.Menu.Items.AddRange(new[] { item });

            editLineItem = item.DropDownItems.Add("Strecke bearbeiten...");
            editLineItem.Enabled = false;
            editLineItem.Click += EditLineItem_Click;

            editTrainsItem = item.DropDownItems.Add("Züge bearbeiten...");
            editTrainsItem.Enabled = false;
            editTrainsItem.Click += EditTrainsItem_Click;

            editTimetableItem = item.DropDownItems.Add("Fahrplan bearbeiten...");
            editTimetableItem.Enabled = false;
            editTimetableItem.Click += EditTimetableItem_Click;
            editTimetableItem.MouseDown += EditTimetableItem_MouseDown;
        }

        private void EditTimetableItem_MouseDown(object sender, MouseEventArgs e)
        {
            //TODO: Remove meta editor
            //if (e.Button == MouseButtons.Middle)
            //{
            //    AttributeEdit mef = new AttributeEdit(info.Timetable);
            //    if (mef.ShowDialog() == DialogResult.OK)
            //        info.SetUnsaved();
            //}
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
            editTimetableItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
        }
    }
}

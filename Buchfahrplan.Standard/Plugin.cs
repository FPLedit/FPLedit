using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Buchfahrplan.Standard
{
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ToolStripItem editLineItem, editTrainsItem, editTimetableItem;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.RegisterExport(new BfplExport());
            info.RegisterImport(new BfplImport());

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
        }

        private void EditTimetableItem_Click(object sender, EventArgs e)
        {
            var ttEdit = new TimetableEditForm();
            ttEdit.Init(info);
            ttEdit.ShowDialog();
            //FileSaved = false;
            //UpdateButtonsEnabled();
        }

        private void EditTrainsItem_Click(object sender, EventArgs e)
        {
            var trEdit = new TrainsEditForm();
            trEdit.Init(info);
            trEdit.ShowDialog();
            //FileSaved = false;
            //UpdateButtonsEnabled();
        }

        private void EditLineItem_Click(object sender, EventArgs e)
        {
            var liEdit = new LineEditForm();
            liEdit.Init(info);
            liEdit.ShowDialog();
            //FileSaved = false;
            //UpdateButtonsEnabled();
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            editLineItem.Enabled = e.Opened;
            editTrainsItem.Enabled = e.Opened;
            editTimetableItem.Enabled = e.Opened;
        }
    }
}

using Buchfahrplan.FileModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace Buchfahrplan
{
    public partial class TrainEditForm : Form
    {
        private List<Train> trains;
        private List<Train> trains_undo;

        public TrainEditForm()
        {
            InitializeComponent();
        }

        public void Init(List<Train> trains)
        {
            this.trains = trains;
            this.trains_undo = trains;
            UpdateTrains();
        }

        private void UpdateTrains()
        {
            topTrainListView.Items.Clear();
            bottomTrainListView.Items.Clear();

            foreach (var train in trains.Where(o => o.Negative == false))
            {
                topTrainListView.Items.Add(new ListViewItem(new[] { 
                    train.Name, 
                    train.Line,
                    train.Locomotive,
                    train.Negative.ToString() }) 
                    { Tag = train });
            }

            foreach (var train in trains.Where(o => o.Negative == true))
            {
                bottomTrainListView.Items.Add(new ListViewItem(new[] { 
                    train.Name, 
                    train.Line,
                    train.Locomotive,
                    train.Negative.ToString() })
                    { Tag = train });
            }
        }

        private void NewEditForm_Load(object sender, EventArgs e)
        {
            topTrainListView.Columns.Add("Zugnummer");
            topTrainListView.Columns.Add("Strecke");
            topTrainListView.Columns.Add("Tfz");
            topTrainListView.Columns.Add("Umgekehrt");

            bottomTrainListView.Columns.Add("Zugnummer");
            bottomTrainListView.Columns.Add("Strecke");
            bottomTrainListView.Columns.Add("Tfz");
            bottomTrainListView.Columns.Add("Umgekehrt");
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            trains = trains_undo;

            this.Close();
        }

        private void EditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }



        private void topDeleteTrainButton_Click(object sender, EventArgs e)
        {
            if (topTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)topTrainListView.Items[topTrainListView.SelectedIndices[0]];
                trains.Remove((Train)item.Tag);

                UpdateTrains();
            }
        }

        private void topChangeNameButton_Click(object sender, EventArgs e)
        {
            string newName = Interaction.InputBox("Bitte einen neuen Namen eingeben:", "Namen ändern");

            if (topTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)topTrainListView.Items[topTrainListView.SelectedIndices[0]];

                trains[trains.IndexOf((Train)item.Tag)].Name = newName;

                UpdateTrains();
            }
        }

        private void topChangeLineButton_Click(object sender, EventArgs e)
        {
            string newLine = Interaction.InputBox("Bitte einen neuen Steckennamen eingeben:", "Streckennamen ändern");

            if (topTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)topTrainListView.Items[topTrainListView.SelectedIndices[0]];

                trains[trains.IndexOf((Train)item.Tag)].Line = newLine;

                UpdateTrains();
            }
        }

        private void topChangeLocomotiveButton_Click(object sender, EventArgs e)
        {
            string newLocomotive = Interaction.InputBox("Bitte einen neuen Namen eingeben:", "Tfz ändern");

            if (topTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)topTrainListView.Items[topTrainListView.SelectedIndices[0]];

                trains[trains.IndexOf((Train)item.Tag)].Locomotive = newLocomotive;

                UpdateTrains();
            }
        }



        private void bottomDeleteTrainButton_Click(object sender, EventArgs e)
        {
            if (bottomTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)bottomTrainListView.Items[bottomTrainListView.SelectedIndices[0]];
                trains.Remove((Train)item.Tag);

                UpdateTrains();
            }
        }

        private void bottomChangeNameButton_Click(object sender, EventArgs e)
        {
            string newName = Interaction.InputBox("Bitte einen neuen Namen eingeben:", "Namen ändern");

            if (bottomTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)bottomTrainListView.Items[bottomTrainListView.SelectedIndices[0]];

                trains[trains.IndexOf((Train)item.Tag)].Name = newName;

                UpdateTrains();
            }
        }

        private void bottomChangeLineButton_Click(object sender, EventArgs e)
        {
            string newLine = Interaction.InputBox("Bitte einen neuen Steckennamen eingeben:", "Streckennamen ändern");

            if (bottomTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)bottomTrainListView.Items[bottomTrainListView.SelectedIndices[0]];

                trains[trains.IndexOf((Train)item.Tag)].Line = newLine;

                UpdateTrains();
            }
        }

        private void bottomChangeLocomotiveButton_Click(object sender, EventArgs e)
        {
            string newLocomotive = Interaction.InputBox("Bitte einen neuen Namen eingeben:", "Tfz ändern");

            if (bottomTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)bottomTrainListView.Items[bottomTrainListView.SelectedIndices[0]];

                trains[trains.IndexOf((Train)item.Tag)].Locomotive = newLocomotive;

                UpdateTrains();
            }
        }
    }
}

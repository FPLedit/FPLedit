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
using Buchfahrplan.Properties;
using Buchfahrplan.Shared;

namespace Buchfahrplan
{
    public partial class TrainEditForm : Form
    {
        private Timetable tt;
        private Timetable tt_undo;

        public TrainEditForm()
        {
            InitializeComponent();

            this.Icon = Resources.programm;
        }

        public void Init(Timetable tt)
        {
            this.tt = tt;
            this.tt_undo = tt;

            topFromToLabel.Text = "Züge " + tt.GetLineName(false);
            topFromToLabel.Text = "Züge " + tt.GetLineName(true);
            UpdateTrains();
        }

        private void UpdateTrains()
        {
            topTrainListView.Items.Clear();
            bottomTrainListView.Items.Clear();

            foreach (var train in tt.Trains.Where(o => o.Negative == false))
            {
                topTrainListView.Items.Add(new ListViewItem(new[] { 
                    train.Name, 
                    train.Line,
                    train.Locomotive,
                    train.Negative.ToString() }) 
                    { Tag = train });
            }

            foreach (var train in tt.Trains.Where(o => o.Negative == true))
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
            tt = tt_undo;

            this.Close();
        }

        private void EditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }



        private void topDeleteTrainButton_Click(object sender, EventArgs e)
        {
            if (topTrainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug löschen");
                return;
            }

            if (topTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)topTrainListView.Items[topTrainListView.SelectedIndices[0]];
                tt.Trains.Remove((Train)item.Tag);

                UpdateTrains();
            }
        }

        private void topChangeNameButton_Click(object sender, EventArgs e)
        {
            if (topTrainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Namen ändern");
                return;
            }

            string newName = Interaction.InputBox("Bitte einen neuen Namen eingeben:", "Namen ändern");

            if (topTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)topTrainListView.Items[topTrainListView.SelectedIndices[0]];

                tt.Trains[tt.Trains.IndexOf((Train)item.Tag)].Name = newName;

                UpdateTrains();
            }
        }

        private void topChangeLineButton_Click(object sender, EventArgs e)
        {
            if (topTrainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Streckennamen ändern");
                return;
            }

            string newLine = Interaction.InputBox("Bitte einen neuen Steckennamen eingeben:", "Streckennamen ändern");

            if (topTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)topTrainListView.Items[topTrainListView.SelectedIndices[0]];

                tt.Trains[tt.Trains.IndexOf((Train)item.Tag)].Line = newLine;

                UpdateTrains();
            }
        }

        private void topChangeLocomotiveButton_Click(object sender, EventArgs e)
        {
            if (topTrainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Tfz ändern");
                return;
            }

            string newLocomotive = Interaction.InputBox("Bitte einen neuen Namen eingeben:", "Tfz ändern");

            if (topTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)topTrainListView.Items[topTrainListView.SelectedIndices[0]];

                tt.Trains[tt.Trains.IndexOf((Train)item.Tag)].Locomotive = newLocomotive;

                UpdateTrains();
            }
        }



        private void bottomDeleteTrainButton_Click(object sender, EventArgs e)
        {
            if (bottomTrainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug löschen");
                return;
            }

            if (bottomTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)bottomTrainListView.Items[bottomTrainListView.SelectedIndices[0]];
                tt.Trains.Remove((Train)item.Tag);

                UpdateTrains();
            }
        }

        private void bottomChangeNameButton_Click(object sender, EventArgs e)
        {
            if (bottomTrainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Namen ändern");
                return;
            }

            string newName = Interaction.InputBox("Bitte einen neuen Namen eingeben:", "Namen ändern");

            if (bottomTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)bottomTrainListView.Items[bottomTrainListView.SelectedIndices[0]];

                tt.Trains[tt.Trains.IndexOf((Train)item.Tag)].Name = newName;

                UpdateTrains();
            }
        }

        private void bottomChangeLineButton_Click(object sender, EventArgs e)
        {
            if (bottomTrainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Streckennamen ändern");
                return;
            }

            string newLine = Interaction.InputBox("Bitte einen neuen Steckennamen eingeben:", "Streckennamen ändern");

            if (bottomTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)bottomTrainListView.Items[bottomTrainListView.SelectedIndices[0]];

                tt.Trains[tt.Trains.IndexOf((Train)item.Tag)].Line = newLine;

                UpdateTrains();
            }
        }

        private void bottomChangeLocomotiveButton_Click(object sender, EventArgs e)
        {
            if (bottomTrainListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Tfz ändern");
                return;
            }

            string newLocomotive = Interaction.InputBox("Bitte einen neuen Namen eingeben:", "Tfz ändern");

            if (bottomTrainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)bottomTrainListView.Items[bottomTrainListView.SelectedIndices[0]];

                tt.Trains[tt.Trains.IndexOf((Train)item.Tag)].Locomotive = newLocomotive;

                UpdateTrains();
            }
        }

        private void topNewTrainButton_Click(object sender, EventArgs e)
        {
            NewTrainForm ntf = new NewTrainForm();
            DialogResult res = ntf.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                Train tra = ntf.NewTrain;
                if (!tra.Negative)
                {
                    foreach (var sta in tt.Stations.OrderBy(s => s.Kilometre))
                    {
                        tra.Arrivals.Add(sta, new DateTime());                        
                    }
                }
                else
                {
                    foreach (var sta in tt.Stations.OrderByDescending(s => s.Kilometre))
                    {
                        tra.Arrivals.Add(sta, new DateTime());
                    }
                }

                tt.Trains.Add(tra);

                UpdateTrains();
            }            
        }
    }
}

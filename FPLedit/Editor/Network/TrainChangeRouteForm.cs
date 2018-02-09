using FPLedit.Shared;
using FPLedit.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Editor.Network
{
    public partial class TrainChangeRouteForm : Form
    {
        private IInfo info;

        private List<Station> path;
        private Train train;

        private TrainChangeRouteForm()
        {
            InitializeComponent();
        }

        public TrainChangeRouteForm(IInfo info, Train tra) : this()
        {
            this.info = info;
            train = tra;
            path = tra.GetPath();

            lineRenderer.StationMovingEnabled = false;
            lineRenderer.FixedStatusString = "Durch Klick Stationen am Anfang/Ende des Laufwegs entfernen";
            lineRenderer.SelectedRoute = -1;
            lineRenderer.SetTimetable(info.Timetable);
            lineRenderer.StationClicked += LineRenderer1_StationClicked;
            lineRenderer.AddHighlight(path);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            var ardps = train.GetArrDeps();

            foreach (var ardp in ardps) // Alle alten Stationen entfernen
                train.RemoveArrDep(ardp.Key);

            train.AddAllArrDeps(path);
            foreach (var ardp in ardps)
            {
                if (!path.Contains(ardp.Key))
                    continue;
                train.SetArrDep(ardp.Key, ardp.Value);
            }

            train.RemoveOrphanedTimes();
            DialogResult = DialogResult.OK;
        }

        private void LineRenderer1_StationClicked(object sender, MouseEventArgs e)
        {
            var sta = (Station)sender;
            if (!path.Contains(sta))
            {
                var pathfinder = new Pathfinder(info.Timetable);
                var pf = pathfinder.GetFromAToB(sta, path.First());
                var pl = pathfinder.GetFromAToB(path.Last(), sta);

                var intersectf = path.Intersect(pf);
                var intersectl = path.Intersect(pl);

                if (pf.Count > pl.Count && intersectl.Count() == 1)
                {
                    pl.Remove(pl.First());
                    path.AddRange(pl); // Am Ende anbauen
                    lineRenderer.AddHighlight(pl);
                }
                else if (intersectf.Count() == 1)
                {
                    pf.Remove(pf.Last());
                    path.InsertRange(0, pf); // Am Anfang anbauen
                    lineRenderer.AddHighlight(pf);
                }
                else
                {
                    MessageBox.Show("Diese Station kann nicht zum Laufweg hinzugefügt werden, da dabei ein verzweigter Laufweg entstehen würde!",
                        "FPLedit");
                }
            }
            else if (sta != path.Last() && sta != path.First())
            {
                MessageBox.Show("Diese Station kann nicht aus dem Laufweg entfernt werden, da sie zwischen Start- und Zielstatation liegt. Nur Start- und Zielstationen können entfernt werden!",
                    "FPLedit");
            }
            else if (path.Count < 3)
            {
                MessageBox.Show("Diese Station kann nicht aus dem Laufweg entfernt werden, da mindestens immer 2 Stationen enthalten sein müssen!",
                    "FPLedit");
            }
            else
            {
                lineRenderer.RemoveHighlight(sta);
                path.Remove(sta);
            }
        }
    }
}

using Eto.Forms;
using FPLedit.Editor.Network;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Network
{
    internal class TrainChangeRouteForm : Dialog<DialogResult>
    {
#pragma warning disable CS0649
        private LineRenderer lineRenderer;
        private Button closeButton;
#pragma warning restore CS0649

        private IInfo info;

        public List<Station> Path { get; private set; }
        private Train train;

        private Station staStart, staEnd; // Set-Mode

        public TrainChangeRouteForm(IInfo info, Train tra)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.info = info;
            train = tra;
            Path = tra.GetPath();
            Title = Title.Replace("{train}", tra.TName);

            lineRenderer.StationMovingEnabled = false;
            lineRenderer.FixedStatusString = "Durch Klick Stationen am Anfang/Ende des Laufwegs entfernen";
            lineRenderer.SelectedRoute = -1;
            lineRenderer.SetTimetable(info.Timetable);
            lineRenderer.StationClicked += ChangeRoute;
            lineRenderer.DisableTopBorder = true;
            lineRenderer.AddHighlight(Path);
        }

        public TrainChangeRouteForm(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.info = info;
            Title = "Fahrtstrecke für neuen Zug auswählen";

            lineRenderer.StationMovingEnabled = false;
            lineRenderer.FixedStatusString = "Startstation auswählen";
            lineRenderer.SelectedRoute = -1;
            lineRenderer.SetTimetable(info.Timetable);
            lineRenderer.StationClicked += SetRoute;

            closeButton.Text = "Weiter >>";
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (train != null)
            {
                var ardps = train.GetArrDeps();

                foreach (var ardp in ardps) // Alle alten Stationen entfernen
                    train.RemoveArrDep(ardp.Key);

                train.AddAllArrDeps(Path);
                foreach (var ardp in ardps)
                {
                    if (!Path.Contains(ardp.Key))
                        continue;
                    train.SetArrDep(ardp.Key, ardp.Value);
                }

                train.RemoveOrphanedTimes();
            }
            Result = DialogResult.Ok;
            Close();
        }

        private void ChangeRoute(object sender, EventArgs e)
        {
            var sta = (Station)sender;
            if (!Path.Contains(sta))
            {
                var pathfinder = new Pathfinder(info.Timetable);
                var pf = pathfinder.GetFromAToB(sta, Path.First());
                var pl = pathfinder.GetFromAToB(Path.Last(), sta);

                var intersectf = Path.Intersect(pf);
                var intersectl = Path.Intersect(pl);

                if (pf.Count > pl.Count && intersectl.Count() == 1)
                {
                    pl.Remove(pl.First());
                    Path.AddRange(pl); // Am Ende anbauen
                    lineRenderer.AddHighlight(pl);
                }
                else if (intersectf.Count() == 1)
                {
                    pf.Remove(pf.Last());
                    Path.InsertRange(0, pf); // Am Anfang anbauen
                    lineRenderer.AddHighlight(pf);
                }
                else
                {
                    MessageBox.Show("Diese Station kann nicht zum Laufweg hinzugefügt werden, da dabei ein verzweigter Laufweg entstehen würde!",
                        "FPLedit");
                }
            }
            else if (sta != Path.Last() && sta != Path.First())
            {
                MessageBox.Show("Diese Station kann nicht aus dem Laufweg entfernt werden, da sie zwischen Start- und Zielstatation liegt. Nur Start- und Zielstationen können entfernt werden!",
                    "FPLedit");
            }
            else if (Path.Count < 3)
            {
                MessageBox.Show("Diese Station kann nicht aus dem Laufweg entfernt werden, da mindestens immer 2 Stationen enthalten sein müssen!",
                    "FPLedit");
            }
            else
            {
                lineRenderer.RemoveHighlight(sta);
                Path.Remove(sta);
            }
        }

        private void SetRoute(object sender, EventArgs e)
        {
            if (staStart == null)
            {
                staStart = (Station)sender;
                lineRenderer.AddHighlight(staStart);
                lineRenderer.FixedStatusString = "Zielstation auswählen";
            }
            else if (staEnd == null)
            {
                staEnd = (Station)sender;

                var pathfinder = new Pathfinder(info.Timetable);
                Path = pathfinder.GetFromAToB(staStart, staEnd);
                lineRenderer.AddHighlight(Path.Skip(1));
                closeButton.Enabled = true;

                lineRenderer.FixedStatusString = "";
            }
        }


        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            Close();
        }
    }
}

using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
    internal class TimetableCheckRunner
    {
        private FForm form;
        private GridView gridView;

        public TimetableCheckRunner(IInfo info)
        {
            var checks = info.GetRegistered<ITimetableCheck>();

            info.FileStateChanged += (s, e) =>
            {
                if (info.Timetable == null)
                    return;

                var list = new List<string>();

                foreach (var check in checks)
                    list.AddRange(check.Check(info.Timetable));

                if (list.Any() && form == null)
                    GetForm().Show();
                if (gridView != null)
                    gridView.DataStore = list;
            };
        }

        private FForm GetForm()
        {
            var stack = new TableLayout(1, 1)
            {
                Padding = new Eto.Drawing.Padding(10),
                Spacing = new Eto.Drawing.Size(5, 5),
            };
            gridView = new GridView();
            gridView.AddColumn<string>(s => s, "Meldung");
            stack.Add(gridView, 0, 0);

            form = new FForm()
            {
                Content = stack,
                Resizable = true,
                Size = new Eto.Drawing.Size(600, 400),
                Title = "Überprüfungen",
            };
            return form;
        }
    }
}

using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
#pragma warning disable CA1001  // Disposable fields are not disposed as this is a windows kept in the background.
    internal sealed class TimetableCheckRunner
    {
        private FForm form;
        private GridView gridView;

        private CancellationTokenSource cancelTokenSource;
        private Task lastTask;

        private readonly object uiLock = new object();

        public TimetableCheckRunner(IPluginInterface pluginInterface)
        {
            var checks = pluginInterface.GetRegistered<ITimetableCheck>();

            pluginInterface.FileStateChanged += (s, e) =>
            {
                if (pluginInterface.Timetable == null)
                    return;

                var clone = pluginInterface.Timetable.Clone();
                
                if (lastTask != null && cancelTokenSource != null && !lastTask.IsCompleted && !cancelTokenSource.IsCancellationRequested)
                    cancelTokenSource.Cancel();

                cancelTokenSource = new CancellationTokenSource();

                lastTask = new Task(tk =>
                {
                    var token = (CancellationToken)tk;
                    
                    var list = new List<string>();

                    foreach (var check in checks)
                    {
                        token.ThrowIfCancellationRequested();
                        list.AddRange(check.Check(clone));
                    }
                    
                    token.ThrowIfCancellationRequested();

                    Application.Instance.Invoke(() =>
                    {
                        lock (uiLock)
                        {
                            if (list.Any() && form == null)
                                GetForm().Show();
                            if (gridView != null && gridView.Visible)
                                gridView.DataStore = list.ToArray();
                        }
                    });
                }, cancelTokenSource.Token, cancelTokenSource.Token);

                lastTask.Start();
            };
            pluginInterface.AppClosing += (s, e) => form?.Close();
        }

        private FForm GetForm()
        {
            var stack = new TableLayout(1, 1)
            {
                Padding = new Eto.Drawing.Padding(10),
                Spacing = new Eto.Drawing.Size(5, 5),
            };
            gridView = new GridView();
            gridView.AddColumn<string>(s => s, T._("Meldung"));
            stack.Add(gridView, 0, 0);

            form = new FForm()
            {
                Content = stack,
                Resizable = true,
                Size = new Eto.Drawing.Size(600, 400),
                Title = T._("Überprüfungen"),
            };
            form.Closing += (s, e) =>
            {
                this.form = null;
                this.gridView = null;
            };
            return form;
        }
    }
#pragma warning restore CA1001
}

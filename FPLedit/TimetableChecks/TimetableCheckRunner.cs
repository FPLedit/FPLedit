using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks;
#pragma warning disable CA1001  // Disposable fields are not disposed as this is a windows kept in the background.
internal sealed class TimetableCheckRunner
{
    private FForm? form;
    private GridView? gridView;

    private CancellationTokenSource? cancelTokenSource;
    private Task? lastTask;

    private readonly object uiLock = new();

    public TimetableCheckRunner(IPluginInterface pluginInterface)
    {
        var checks = pluginInterface.GetRegistered<ITimetableCheck>();

        pluginInterface.FileStateChanged += (_, _) =>
        {
            if (pluginInterface.TimetableMaybeNull == null!)
                return;

            if (lastTask != null && cancelTokenSource != null && !lastTask.IsCompleted && !cancelTokenSource.IsCancellationRequested)
                cancelTokenSource.Cancel();

            cancelTokenSource = new CancellationTokenSource();

            lastTask = new Task(tk =>
            {
                var token = (CancellationToken)tk!;

                // First clone the timetable. Doing this off UI-thread is dangerous, but otherwise we will stall the UI
                // rendering a lot for big files.
                if (pluginInterface.TimetableMaybeNull == null!)
                    return;
                var clone = pluginInterface.Timetable.Clone();
                if (token.IsCancellationRequested) return;

                // Execute all checks.
                var list = new List<TimetableCheckResult>();
                foreach (var check in checks)
                {
                    if (token.IsCancellationRequested) return;
                    list.AddRange(check.Check(clone));
                }

                if (!list.Any() || token.IsCancellationRequested)
                    return;

                Application.Instance.Invoke(() =>
                {
                    lock (uiLock)
                    {
                        if (form == null)
                            GetForm().Show();
                        if (token.IsCancellationRequested) return;
                        if (gridView != null && gridView.Visible)
                            gridView.DataStore = list.ToArray();
                    }
                });
            }, cancelTokenSource.Token, cancelTokenSource.Token);

            lastTask.Start();
        };
        pluginInterface.AppClosing += (_, _) => form?.Close();
    }

    private FForm GetForm()
    {
        var stack = new TableLayout(1, 1)
        {
            Padding = new Eto.Drawing.Padding(10),
            Spacing = new Eto.Drawing.Size(5, 5),
        };
        gridView = new GridView();
        gridView.AddFuncColumn<TimetableCheckResult>(s => s.Display, T._("Meldung"));
        // This allows the selection of the last row on Wpf, see Eto#2443.
        if (gridView.Platform.IsGtk) gridView.AllowEmptySelection = false;
        stack.Add(gridView, 0, 0);

        form = new FForm()
        {
            Content = stack,
            Resizable = true,
            Size = new Eto.Drawing.Size(600, 400),
            Title = T._("Überprüfungen"),
        };
        form.Closing += (_, _) =>
        {
            this.form = null;
            this.gridView = null;
        };
        return form;
    }
}
#pragma warning restore CA1001
using FPLedit.Shared;
using System;

namespace FPLedit;

internal sealed class FileState : IFileState
{
    private readonly object lockObject = new ();
    private bool inhibitEvents = false;
    private bool opened, saved;
    private string? fileName;

    public bool Opened { get => opened; set => TriggerEvent(opened = value); }

    public bool Saved { get => saved; set => TriggerEvent(saved = value); }

    public bool LineCreated { get; private set; }

    public bool TrainsCreated { get; private set; }

    public bool CanGoBack { get; private set; }

    public string? FileName
    {
        get => fileName;
        set
        {
            lock (lockObject)
            {
                if (value != fileName)
                    FileNameRevisionCounter++;
                TriggerEvent(fileName = value);
            }
        }
    }

    public int SelectedRoute { get; set; }

    internal int RevisionCounter { get; private set; }

    internal int FileNameRevisionCounter { get; private set; }

    public void UpdateMetaProperties(Timetable? tt, UndoManager undo)
    {
        LineCreated = tt?.Stations.Count > 1; // Mind. 2 Bahnhöfe
        TrainsCreated = tt?.Trains.Count > 0;
        CanGoBack = undo.CanGoBack;
    }

    private void TriggerEvent(object? o)
    {
        lock (lockObject)
        {
            if (inhibitEvents) return;
            RevisionCounter++;
            FileStateInternalChanged?.Invoke(this, new FileStateChangedEventArgs(this));
        }
    }

    internal void BatchMutate(Action<FileState> action)
    {
        lock (lockObject)
        {
            inhibitEvents = true;
            action(this);
            inhibitEvents = false;
            TriggerEvent(null);
        }
    }

    internal event EventHandler<FileStateChangedEventArgs>? FileStateInternalChanged;
}
using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared
{
    public interface IPluginInterface
    {
        Timetable Timetable { get; }

        IFileState FileState { get; }
        void SetUnsaved();

        object BackupTimetable();
        void RestoreTimetable(object backupHandle);
        void ClearBackup(object backupHandle);

        // Regsitry
        void Register<T>(T elem);
        T[] GetRegistered<T>();

        // FileHandling
        void Open();
        void Save(bool forceSaveAs);
        void Reload();
        string GetTemp(string filename);

        // Undo
        void Undo();
        void StageUndoStep();

        dynamic Menu { get; }
        dynamic RootForm { get; }

        ILog Logger { get; }
        ISettings Settings { get; }
        ITemplateManager TemplateManager { get; }

        event EventHandler<FileStateChangedEventArgs> FileStateChanged;
        event EventHandler ExtensionsLoaded;
        event EventHandler FileOpened;
        event EventHandler AppClosing;
    }
}
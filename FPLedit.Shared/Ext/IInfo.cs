using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared
{
    public interface IInfo
    {
        Timetable Timetable { get; set; }

        IFileState FileState { get; }

        void SetUnsaved();

        void BackupTimetable();

        void RestoreTimetable();

        void ClearBackup();

        // Regsitry
        void Register<T>(T obj);

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
    }

    public class FileStateChangedEventArgs : EventArgs
    {
        public IFileState FileState { get; private set; }

        public FileStateChangedEventArgs(IFileState state)
        {
            FileState = state;
        }
    }
}
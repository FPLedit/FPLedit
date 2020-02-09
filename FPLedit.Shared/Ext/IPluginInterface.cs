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
        string ExecutablePath { get; }
        string ExecutableDir { get; }

        // Undo
        void Undo();
        void StageUndoStep();

        dynamic Menu { get; }
        dynamic RootForm { get; }
        /// <summary>
        /// Use this instead of <see cref="Menu"/>.HelpMenu.
        /// </summary>
        dynamic HelpMenu { get; }

        ILog Logger { get; }
        ISettings Settings { get; }
        /// <remarks>
        /// This property is only available after the <see cref="ExtensionsLoaded"/> event is invoked and thus cannot be used in <see cref="IPlugin.Init"/>.
        /// </remarks>
        ITemplateManager TemplateManager { get; }

        event EventHandler<FileStateChangedEventArgs> FileStateChanged;
        event EventHandler ExtensionsLoaded;
        event EventHandler FileOpened;
        event EventHandler AppClosing;
    }
}
using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared
{
    public interface IPluginInterface : IReducedPluginInterface, IUiPluginInterface
    {
        Timetable Timetable { get; }

        IFileState FileState { get; }
        void SetUnsaved();

        object BackupTimetable();
        void RestoreTimetable(object backupHandle);
        void ClearBackup(object backupHandle);

        // Regsitry (active)
        void Register<T>(T elem);

        // FileHandling
        void Open();
        void Save(bool forceSaveAs);
        void Reload();

        // Undo
        void Undo();
        void StageUndoStep();

        event EventHandler<FileStateChangedEventArgs> FileStateChanged;
        event EventHandler ExtensionsLoaded;
        event EventHandler FileOpened;
        event EventHandler AppClosing;
    }

    public interface IReducedPluginInterface
    {
        // Registry (passive)
        T[] GetRegistered<T>();
        
        ILog Logger { get; }
        ISettings Settings { get; }
        
        /// <remarks>
        /// This property is only available after the <see cref="IPluginInterface.ExtensionsLoaded"/> event is invoked and thus cannot be used in <see cref="IPlugin.Init"/>.
        /// </remarks>
        ITemplateManager TemplateManager { get; }
        
        string GetTemp(string filename);
        string ExecutablePath { get; }
        string ExecutableDir { get; }
    }

    public interface IUiPluginInterface
    {
        dynamic Menu { get; }
        dynamic RootForm { get; }
        /// <summary>
        /// Use this instead of <see cref="Menu"/>.HelpMenu.
        /// </summary>
        dynamic HelpMenu { get; }

    }
}
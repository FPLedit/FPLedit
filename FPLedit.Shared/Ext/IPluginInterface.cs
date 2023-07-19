using FPLedit.Shared.Templating;
using System;

namespace FPLedit.Shared;

public interface IPluginInterface : IReducedPluginInterface, IUiPluginInterface
{
    Timetable Timetable { get; }
    Timetable? TimetableMaybeNull { get; }

    IFileState FileState { get; }
    void SetUnsaved();

    object BackupTimetable();
    void RestoreTimetable(object backupHandle);
    void ClearBackup(object backupHandle);

    // FileHandling
    void Open();
    void Save(bool forceSaveAs);
    void Reload();

    // Undo
    void Undo();
    void StageUndoStep();

    new ISettings Settings { get; }

    event EventHandler<FileStateChangedEventArgs> FileStateChanged;
    event EventHandler ExtensionsLoaded;
    event EventHandler FileOpened;
    event EventHandler AppClosing;
}

public interface IComponentRegistry
{
    void Register<T>(T elem) where T : IRegistrableComponent;
}

public interface IReducedPluginInterface
{
    // Registry (passive)
    T[] GetRegistered<T>();

    ILog Logger { get; }
    IReadOnlySettings Settings { get; }

    /// <remarks>
    /// This property is only available after the <see cref="IPluginInterface.ExtensionsLoaded"/> event is invoked and thus cannot be used in <see cref="IPlugin.Init"/>.
    /// </remarks>
    ITemplateManager TemplateManager { get; }

    string GetTemp(string filename);
    string ExecutableDir { get; }
}

public interface IUiPluginInterface
{
    dynamic Menu { get; }
    dynamic RootForm { get; }

    void OpenUrl(string address, bool isInternal = false);
}
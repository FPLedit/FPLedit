using FPLedit.Shared.Templating;
using System;

namespace FPLedit.Shared;

public interface IPluginInterface : IReducedPluginInterface, IUiPluginInterface
{
    /// <summary>
    /// This will provide the current timetable instance, and will throw if the current instance is unset (i.e. no file is opened).
    /// </summary>
    /// <remarks>Use <see cref="TimetableMaybeNull"/> to check for <see langword="null" />.</remarks>
    /// <exception cref="NullReferenceException">No current timetable exists, i.e. no file is opened.</exception>
    Timetable Timetable { get; }
    /// <summary>
    /// This will provide the current timetable instance, or <see langword="null" /> if the current instance is unset (i.e. no file is opened).
    /// </summary>
    Timetable? TimetableMaybeNull { get; }

    /// <summary>
    /// The current application-wide file state (this is also accessible if no file is opened).
    /// </summary>
    IFileState FileState { get; }
    /// <summary>
    /// Mark the current timetable as unsaved
    /// </summary>
    /// <exception cref="NullReferenceException">No current timetable exists, i.e. no file is opened.</exception>
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

    event EventHandler<FileStateChangedEventArgs>? FileStateChanged;
    event EventHandler? ExtensionsLoaded;
    event EventHandler? FileOpened;
    event EventHandler? AppClosing;
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
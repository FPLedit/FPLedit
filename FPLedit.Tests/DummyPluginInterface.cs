using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
#pragma warning disable CS0067

namespace FPLedit.Tests
{
    internal class DummyPluginInterface : IPluginInterface
    {
        public ISettings settings;
        public DummyPluginInterface()
        {
            Logger = new TestLogger();
            settings = new TestSettings(new Dictionary<string, string>());
            Registry = new RegisterStore();
        }

        public string GetTemp(string filename)
        {
            throw new NotImplementedException();
        }

        public string ExecutablePath => throw new NotImplementedException();
        public string ExecutableDir { get; set; } = null!;

        public T[] GetRegistered<T>() => Registry.GetRegistered<T>();

        public ILog Logger { get; }

        public Timetable Timetable => throw new NotImplementedException();
        public Timetable? TimetableMaybeNull => throw new NotImplementedException();

        public IFileState FileState => throw new NotImplementedException();

        public void SetUnsaved()
        {
            throw new NotImplementedException();
        }

        public object BackupTimetable() => throw new NotImplementedException();

        public void RestoreTimetable(object backupHandle)
        {
            throw new NotImplementedException();
        }

        public void ClearBackup(object backupHandle)
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void Save(bool forceSaveAs)
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }

        public void Undo()
        {
            throw new NotImplementedException();
        }

        public void StageUndoStep()
        {
            throw new NotImplementedException();
        }

        ISettings IPluginInterface.Settings => settings;

        public event EventHandler<FileStateChangedEventArgs>? FileStateChanged;
        public event EventHandler? ExtensionsLoaded;
        public event EventHandler? FileOpened;
        public event EventHandler? AppClosing;
        public IReadOnlySettings Settings => settings;
        public ITemplateManager TemplateManager => throw new NotImplementedException();

        public int HadWarning(string search) => ((TestLogger) Logger).HadWarning(search);

        public RegisterStore Registry { get; }

        public dynamic Menu => throw new NotImplementedException();

        public dynamic RootForm => throw new NotImplementedException();
        public void OpenUrl(string address, bool isInternal = false)
        {
            throw new NotImplementedException();
        }

        public dynamic HelpMenu => throw new NotImplementedException();
    }

    public class TestSettings : ISettings
    {
        private readonly Dictionary<string, string> settings;

        public TestSettings(Dictionary<string, string> settings)
        {
            this.settings = settings;
        }

        public bool IsReadonly => false;

        public T? Get<T>(string key, T? defaultValue = default)
        {
            settings.TryGetValue(key, out var val);
            if (val != null)
                return (T) Convert.ChangeType(val, typeof(T));
            return defaultValue;
        }
        public T? GetEnum<T>(string key, T? defaultValue = default) where T : Enum
        {
            var underlying = Enum.GetUnderlyingType(typeof(T));
            var x = (int) Convert.ChangeType(defaultValue, underlying)!;
            return (T) Enum.ToObject(typeof(T), Get(key, x));
        }
        public bool KeyExists(string key) => settings.ContainsKey(key);
        public void Set(string key, string value) => settings[key] = value;
        public void Set(string key, bool value) => Set(key, value.ToString().ToLower());
        public void Set(string key, int value) => Set(key, value.ToString());
        public void SetEnum<T>(string key, T value) where T : Enum
        {
            var underlying = Enum.GetUnderlyingType(typeof(T));
            var x = (int) Convert.ChangeType(value, underlying);
            Set(key, x);
        }
        public void Remove(string key) => settings.Remove(key);
    }

    public class TestLogger : ILog
    {
        private List<string> warnings = new List<string>();

        public int HadWarning(string search)
        {
            var warns = warnings.Where(s => s.Contains(search)).ToList();
            warnings.RemoveAll(w => warns.Contains(w));
            return warns.Count;
        }
        
        public bool CanAttach => false;

        public void AttachLogger(ILog other)
        {
        }
        
        public void Error(string message) => throw new LogErrorException("Error: " + message);

        public void Warning(string message) => warnings.Add(message);

        public void Info(string message) => Console.WriteLine("Info: " + message);

        public void Debug(string message) => Console.WriteLine("Debug: " + message);

        public void LogException(Exception e) => throw e;
    }

    public class LogWarningException : Exception
    {
        public LogWarningException()
        {
        }

        public LogWarningException(string message) : base(message)
        {
        }
    }

    public class LogErrorException : Exception
    {
        public LogErrorException()
        {
        }

        public LogErrorException(string message) : base(message)
        {
        }
    }
}
using System;
using System.Collections.Generic;
using FPLedit.Shared.Templating;

namespace FPLedit.Shared.Tests.TestClasses
{
    public class DummyPluginInterface : IReducedPluginInterface
    {
        public DummyPluginInterface()
        {
            Logger = new TestLogger();
            Settings = new TestSettings(new Dictionary<string, string>());
        }

        public ICacheFile Cache => throw new NotImplementedException();

        public string GetTemp(string filename)
        {
            throw new NotImplementedException();
        }

        public string ExecutablePath => throw new NotImplementedException();
        public string ExecutableDir => throw new NotImplementedException();

        public T[] GetRegistered<T>() => Array.Empty<T>();

        public ILog Logger { get; }
        public IReadOnlySettings Settings { get; }
        public ITemplateManager TemplateManager => throw new NotImplementedException();
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
        public bool CanAttach => false;

        public void AttachLogger(ILog other)
        {
        }
        
        public void Error(string message) => throw new LogErrorException("Error: " + message);

        public void Warning(string message) => throw new LogWarningException("Warning: " + message);

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
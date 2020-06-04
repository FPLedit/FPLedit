using System;

namespace FPLedit.Shared
{
    public interface ISettings : IReadOnlySettings
    {
        void Set(string key, string value);

        void Set(string key, bool value);

        void Set(string key, int value);

        void SetEnum<T>(string key, T value) where T : Enum;

        void Remove(string key);
    }
    
    public interface IReadOnlySettings
    {
        bool IsReadonly { get; }
        
        T Get<T>(string key, T defaultValue = default);

        T GetEnum<T>(string key, T defaultValue = default) where T : Enum;

        bool KeyExists(string key);
    }
}

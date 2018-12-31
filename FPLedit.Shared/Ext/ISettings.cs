using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface ISettings
    {
        T Get<T>(string key, T defaultValue = default);

        T GetEnum<T>(string key, T defaultValue = default) where T : Enum;

        bool KeyExists(string key);

        void Set(string key, string value);

        void Set(string key, bool value);

        void Set(string key, int value);

        void SetEnum<T>(string key, T value) where T : Enum;

        void Remove(string key);
    }
}

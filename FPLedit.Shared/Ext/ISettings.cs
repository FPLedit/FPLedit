using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface ISettings
    {
        T Get<T>(string key);

        T Get<T>(string key, T defaultValue);

        bool KeyExists(string key);

        void Set(string key, string value);

        void Set(string key, bool value);

        void Set(string key, int value);

        void Remove(string key);
    }
}

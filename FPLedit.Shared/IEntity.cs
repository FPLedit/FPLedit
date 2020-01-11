using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [TemplateSafe]
    public interface IEntity
    {
        T GetAttribute<T>(string key, T defaultValue = default);

        void SetAttribute(string key, string value);

        void RemoveAttribute(string key);
    }
}

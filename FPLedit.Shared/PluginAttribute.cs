using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginAttribute : Attribute
    {
        private string _name;
        public string Name => _name;

        public PluginAttribute(string name)
        {
            _name = name;
        }
    }
}

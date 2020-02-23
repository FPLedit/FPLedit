using System;

namespace FPLedit.Shared.Templating
{
    public interface ITemplateWhitelistEntry
    {
        string TemplateType { get;  }
        Type GetWhitelistType();
    }
    
    public class TemplateWhitelistEntry<T> : ITemplateWhitelistEntry
    {
        public string TemplateType { get; }
        
        /// <summary>
        /// Creates a new instance to register as <see cref="ITemplateWhitelistEntry"/>.
        /// </summary>
        /// <param name="templateType">Target template type to register this .NET type for, or 'null' for all.</param>
        public TemplateWhitelistEntry(string templateType)
        {
            TemplateType = templateType;
        }

        public Type GetWhitelistType() 
            => GetType().GenericTypeArguments[0];
    }
}
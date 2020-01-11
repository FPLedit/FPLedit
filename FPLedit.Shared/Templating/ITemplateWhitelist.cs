using System;

namespace FPLedit.Shared.Templating
{
    public interface ITemplateWhitelist
    {
        string TemplateType { get;  }
        Type GetWhitelistType();
    }
    
    public class TemplateWhitelist<T> : ITemplateWhitelist
    {
        public string TemplateType { get; }
        
        /// <summary>
        /// Creates a new instance to register as <see cref="ITemplateWhitelist"/>.
        /// </summary>
        /// <param name="templateType">Target template type to register this .NET type for, or 'null' for all.</param>
        public TemplateWhitelist(string templateType)
        {
            TemplateType = templateType;
        }

        public Type GetWhitelistType() 
            => GetType().GenericTypeArguments[0];
    }
}
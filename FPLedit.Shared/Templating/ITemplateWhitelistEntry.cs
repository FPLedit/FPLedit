using System;

namespace FPLedit.Shared.Templating
{
    /// <summary>
    /// The type whitelisted with this registrable component will be availabvle in templates of the given TemplateType.
    /// </summary>
    public interface ITemplateWhitelistEntry : IRegistrableComponent
    {
        /// <summary>
        /// Target template type to register this .NET type for, or 'null' for all.
        /// </summary>
        string TemplateType { get; }
        /// <summary>
        /// Return the whitelisted type. Must always return the same value.
        /// </summary>
        Type GetWhitelistType();
    }
    
    /// <summary>
    /// Default implementation to provide an easy way to implement <see cref="ITemplateWhitelistEntry"/>.
    /// </summary>
    /// <typeparam name="T">Type to be whitelisted.</typeparam>
    // ReSharper disable once UnusedTypeParameter
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

        /// <summary>
        /// Return the whitelisted type. Will always return the same value.
        /// </summary>
        public Type GetWhitelistType() 
            => GetType().GenericTypeArguments[0];
    }
}
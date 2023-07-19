using System;

namespace FPLedit.Shared.Templating;

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
    /// Return the whitelisted type(s). Must always return the same value.
    /// </summary>
    Type[] WhitelistTypes { get; }
}

/// <summary>
/// Default implementation to provide an easy way to implement <see cref="ITemplateWhitelistEntry"/>.
/// </summary>
public record TemplateWhitelistEntry(string TemplateType, params Type[] WhitelistTypes) : ITemplateWhitelistEntry;
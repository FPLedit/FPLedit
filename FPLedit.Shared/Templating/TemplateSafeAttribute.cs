using System;

namespace FPLedit.Shared.Templating;

/// <summary>
/// Marks a class, enum or struct as safe to use in templates.
/// </summary>
/// <remarks>
/// This attribute has no effect on types outside the <see cref="FPLedit.Shared"/> assembly. Use the registrable
/// component <see cref="ITemplateWhitelistEntry"/> to whitelist extension types.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
public sealed class TemplateSafeAttribute : Attribute
{
    public bool AllowExtensionMethods { get; init; } = false;
}
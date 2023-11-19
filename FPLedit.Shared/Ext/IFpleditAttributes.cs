using System;

namespace FPLedit.Shared;

/// <summary>
/// Declares some XML attributes on a core FPLedit entity as FPLedit-internal.
/// </summary>
/// <remarks>This is used to remove all FPLedit-specific attributes from linear timetables, if needed.</remarks>
public interface IFpleditAttributes : IRegistrableComponent
{
    /// <summary>
    /// Type of an <see cref="Entity"/> with FPLedit internal XML attributes
    /// </summary>
    Type BaseType { get; }
    /// <summary>
    /// List of XML attribute names that are declared as FPLedit-internal.
    /// </summary>
    string[] Attributes { get; }
}

/// <summary>
/// Default implementation to declare <see cref="IFpleditAttributes"/>.
/// </summary>
public record FpleditAttributes(Type BaseType, params string[] Attributes) : IFpleditAttributes;
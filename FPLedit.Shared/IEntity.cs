﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FPLedit.Shared;

/// <summary>
/// Base type for all entities that are used in a timetable object model. Provides basic capabilities foir storing
/// attributes, children and interacting with underlying <see cref="XMLEntity"/>s.
/// </summary>
[Templating.TemplateSafe]
public interface IEntity
{
    XMLEntity XMLEntity { get; }

    [NotNull] Timetable? ParentTimetable { get; set; }

    /// <summary>
    /// Gets or sets all XML attributes of the underlying XML node.
    /// </summary>
    Dictionary<string, string> Attributes { get; set; }
    /// <summary>
    /// Gets all children of the underlying XML node. The collection can be modified.
    /// </summary>
    IList<XMLEntity> Children { get; }

    /// <summary>
    /// Get an attribute of the underlying XML node.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [return: NotNullIfNotNull("defaultValue")]
    T? GetAttribute<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Set or add an attribute of the underlying XML node.
    /// </summary>
    /// <param name="key">The attribute name that will be set.</param>
    /// <param name="value">The new value of the attribute.</param>
    void SetAttribute(string key, string value);

    /// <summary>
    /// Remove an attribute of the underlying XML node.
    /// </summary>
    /// <param name="key">The attribute name that will be removed.</param>
    void RemoveAttribute(string key);
}
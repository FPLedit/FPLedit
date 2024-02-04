using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.Linq;
using Force.DeepCloner.Helpers;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared;

/// <summary>
/// A RouteValueCollection (RVC) allows to define properties which allow for different values for each individual
/// route of a Station. This type of RVC is associated with an XML attribute.
/// </summary>
/// <remarks>
/// <para>Reference integrity is not guaranteed.</para>
/// <para>IMPORTANT! Do never rely on this to store data that is critical to the inetgrity of the stored data,
/// if the RVC is not part of the <see cref="Station"/> class. Only members of that class will be automatically
/// updated when routes are removed, or splitted!</para>
/// <para>See <see cref="StandaloneRouteValueCollection{T}"/> if you need to store data associated to routes and
/// you don't need to store the result in an XML attribute.</para>
/// </remarks>
/// <typeparam name="T">Any type that is transformable to a string with reasonable effort.</typeparam>
[Templating.TemplateSafe]
public class RouteValueCollection<T> : IRouteValueCollection<T>
{
    private static readonly EscapeSplitHelper escape = new(';');

    private readonly IEntity entity;
    private readonly Dictionary<int, T> values;
    private readonly Timetable tt;
    private readonly string attr, defaultVal;
    private readonly Func<string, T> convTo;
    private readonly Func<T, string?> convFrom;
    private readonly bool optional;
    private readonly T? convDefault;

    /// <inheritdoc />
    public IReadOnlyDictionary<int, T> Values => new ReadOnlyDictionary<int, T>(values);

    /// <summary>
    /// Creates a standalone copy of this RVC.
    /// </summary>
    public IRouteValueCollection<T> ToStandalone()
        => new StandaloneRouteValueCollection<T>(DeepCloner.CloneObject(values), convDefault);

    /// <summary>
    /// Copies all values from a (standalone) RVC back into this instance. This clears all values of this instance before.
    /// </summary>
    public void FromStandalone(IRouteValueCollection<T> standalone)
    {
        values.Clear();
        foreach (var val in standalone.Values)
            values[val.Key] = val.Value;
        Write();
    }

    /// <summary>
    /// Creates a new RVC.
    /// </summary>
    /// <param name="e">The entity this RVC should operate on, read values from &amp; write vTalues to.</param>
    /// <param name="tt">The parent timetable of the entity <paramref name="e"/>.</param>
    /// <param name="attr">The XML attribute name this RVC should read from and write to.</param>
    /// <param name="defaultVal">Default value that is used when the given attribute is not present. As serialized string.</param>
    /// <param name="convTo">Function to serialize <typeparamref name="T"/> to a string. Special rules apply. See remarks.</param>
    /// <param name="convFrom">Function to deserialize <typeparamref name="T"/> from a string. Special rules apply. See remarks.</param>
    /// <param name="optional">Specifies if this attribute is optional.</param>
    /// <remarks>
    /// The character ";" will be escaped in the serialized string.
    /// </remarks>
    public RouteValueCollection(IEntity e, Timetable tt, string attr, string defaultVal, Func<string, T> convTo, Func<T, string?> convFrom, bool optional = true)
    {
        this.attr = attr;
        this.convFrom = convFrom;
        this.convTo = convTo;
        this.defaultVal = defaultVal;
        this.optional = optional;
        convDefault = convTo(defaultVal);

        entity = e;
        values = new Dictionary<int, T>();
        this.tt = tt;
        if (tt.Type == TimetableType.Linear)
            ParseLinear();
        else
            ParseNetwork();
    }

    /// <summary>
    /// This method does nothing, but can be used to test for errors if the RVC is constructed on-demand.
    /// </summary>
    public void TestForErrors()
    {
    }

    /// <inheritdoc />
    public T? GetValue(int route)
    {
        if (values.TryGetValue(route, out var val))
            return val;
        return convDefault;
    }

    /// <inheritdoc />
    public void SetValue(int route, T? val)
    {
        values[route] = val!;
        Write();
    }

    /// <inheritdoc />
    public bool RemoveValue(int route, out T? oldValue)
    {
        var didRemove = values.Remove(route, out oldValue);
        if (didRemove) Write();
        return didRemove;
    }

    /// <inheritdoc />
    public bool RemoveValue(int route) => RemoveValue(route, out _);

    /// <summary>
    /// Parse the attribute data from a multi-route context (e.g. Network timetable).
    /// </summary>
    private void ParseNetwork()
    {
        var toParse = entity.GetAttribute(attr, "");
        var rts = escape.SplitEscaped(toParse);
        foreach (var p in rts)
        {
            var parts = p.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

            var route = int.Parse(parts[0]);
            var value = convTo(parts.Length == 2 ? parts[1] : defaultVal);
            values.Add(route, value);
        }
    }

    /// <summary>
    /// Parse the attribute if there is only one route and we are not in a multi-route context (e.g. linear timetable).
    /// </summary>
    private void ParseLinear()
    {
        var toParse = entity.GetAttribute<string>(attr);
        if (optional && toParse == null)
            return;

        toParse ??= defaultVal;
        values.Add(Timetable.LINEAR_ROUTE_ID, convTo(toParse));
    }

    /// <summary>
    /// Write all values back to the XML attribute.
    /// </summary>
    /// <param name="forceType">Force either network or linear mode (only to be used by conversions!).</param>
    public void Write(TimetableType? forceType = null)
    {
        // Skip, if collection is empty & attribute is optional
        if (optional && !values.Any())
            return;

        string? text;
        var t = forceType ?? tt.Type;
        if (t == TimetableType.Linear)
            text = convFrom(GetValue(Timetable.LINEAR_ROUTE_ID)!);
        else
        {
            var posStrings = values.Select(kvp => kvp.Key + ":" + convFrom(kvp.Value));
            text = escape.JoinEscaped(posStrings);
        }

        entity.SetAttribute(attr, text ?? "");
        if (t == TimetableType.Linear && optional && text == null)
            entity.RemoveAttribute(attr);
    }

    /// <inheritdoc />
    public bool ContainsValue(T value) => values.ContainsValue(value);

    /// <inheritdoc />
    public void ReplaceAllValues([DisallowNull] T oldVal, T? newVal)
    {
        for (int i = 0; i < values.Count; i++)
        {
            var kvp = values.ElementAt(i);
            if (oldVal.Equals(kvp.Value))
                SetValue(kvp.Key, newVal);
        }
    }

    /// <inheritdoc />
    public void SwapRouteId(int oldRoute, int newRoute)
    {
        if (RemoveValue(oldRoute, out var val))
            return;
        SetValue(newRoute, val);
    }
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FPLedit.Shared;

/// <inheritdoc />
public sealed class StandaloneRouteValueCollection<T> : IRouteValueCollection<T>
{
    private readonly Dictionary<int, T> values;
    [MaybeNull] private readonly T defaultValue;

    internal StandaloneRouteValueCollection(Dictionary<int, T> values, T? defaultValue)
    {
        this.values = values;
        this.defaultValue = defaultValue!;
    }
        
    /// <inheritdoc />
    public IReadOnlyDictionary<int, T> Values => new ReadOnlyDictionary<int, T>(values);

    /// <inheritdoc />
    public T? GetValue(int route)
    {
        if (values.TryGetValue(route, out var val))
            return val;
        return defaultValue;
    }

    /// <inheritdoc />
    public void SetValue(int route, T? val)
    {
        values[route] = val!;
    }
        
    /// <inheritdoc />
    public bool RemoveValue(int route, out T? oldValue)
    {
        return values.Remove(route, out oldValue);
    }

    /// <inheritdoc />
    public bool RemoveValue(int route) => RemoveValue(route, out _);

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
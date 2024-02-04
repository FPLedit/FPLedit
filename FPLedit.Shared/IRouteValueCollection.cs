using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FPLedit.Shared;

/// <summary>
/// A RouteValueCollection (RVC) allows to define properties which allow for different values for each individual
/// route of a Station.
/// </summary>
/// <remarks>Reference integrity is not guaranteed.</remarks>
/// <typeparam name="T">Any type that is transformable to a string with reasonable effort.</typeparam>
public interface IRouteValueCollection<T> : IRouteValueCollection
{
    /// <summary>
    /// Returns all current values of this RouteValueCollection.
    /// </summary>
    IReadOnlyDictionary<int, T> Values { get; }

    /// <summary>
    /// Return the value - or the default value, if no value has been set - corresponding to the given route.
    /// </summary>
    T? GetValue(int route);

    /// <summary>
    /// Set the value corresponding to the given route.
    /// </summary>
    void SetValue(int route, T? val);

    /// <summary>
    /// Removes the value for the given route.
    /// </summary>
    /// <param name="route"></param>
    /// <param name="oldValue">the removed value, if <see langword="true" /> has been returned.</param>
    /// <returns><see langword="true" /> if something was removed.</returns>
    bool RemoveValue(int route, out T? oldValue);

    /// <summary>
    /// Returns whether this RVC contains the given value at any route.
    /// </summary>
    bool ContainsValue(T value);

    /// <summary>
    /// Replace all occurences of <paramref name="oldVal"/> with <paramref name="newVal"/> on all routes.
    /// </summary>
    void ReplaceAllValues([DisallowNull] T oldVal, T? newVal);
}

/// <summary>
/// A RouteValueCollection (RVC) allows to define properties which allow for different values for each individual
/// route of a Station.
/// </summary>
public interface IRouteValueCollection
{
    /// <summary>
    /// Removes the value for the given route.
    /// </summary>
    /// <returns><see langword="true" /> if something was removed.</returns>
    bool RemoveValue(int route);

    /// <summary>
    /// Removes the value of the route <paramref name="oldRoute"/> and re-adds it to <paramref name="newRoute"/>.
    /// </summary>
    void SwapRouteId(int oldRoute, int newRoute);
}
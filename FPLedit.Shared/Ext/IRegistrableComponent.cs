namespace FPLedit.Shared
{
    // ReSharper disable once CA1040
    /// <summary>
    /// Base interface for all registrable components, which provides no functionalöity itself.
    /// All interfaces or classes that should be used with <see cref="IComponentRegistry.Register{T}"/> must implement this interface.
    /// </summary>
#pragma warning disable CA1040
    public interface IRegistrableComponent
    {
    }
#pragma warning restore CA1040
}
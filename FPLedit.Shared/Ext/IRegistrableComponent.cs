namespace FPLedit.Shared
{
    /// <summary>
    /// Base interface for all registrable components, which provides no functionalöity itself.
    /// All interfaces or classes that should be used with <see cref="IComponentRegistry.Register{T}"/> must implement this interface.
    /// </summary>
    public interface IRegistrableComponent { }
}
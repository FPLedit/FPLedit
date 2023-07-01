namespace FPLedit.Shared;

/// <summary>
/// Indicates that this extensions supports using virtual routes. The main application should thus provide a user interface to edit them.
/// </summary>
public interface ISupportsVirtualRoutes : IRegistrableComponent { }

/// <inheritdoc />
public sealed class SupportsVirtualRoutes : ISupportsVirtualRoutes { }
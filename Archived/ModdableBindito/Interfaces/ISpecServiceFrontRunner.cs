namespace ModdableBindito;

/// <summary>
/// Multibound services with this interface will run before <see cref="ISpecService"/>.
/// <remarks>It must not depend on <see cref="ISpecService"/>.</remarks>
/// </summary>
public interface ISpecServiceFrontRunner
{
}

/// <summary>
/// Indicate that this service can override the default spec loading.
/// </summary>
public interface ISpecLoader : ISpecServiceFrontRunner
{
    /// <summary>
    /// The priority in which this service will be called. Higher ones are called first.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Get a single spec of type T.
    /// </summary>
    /// <typeparam name="T">The type of the spec to get.</typeparam>
    /// <param name="spec">The output parameter that will contain the spec if found.</param>
    /// <returns>True if the spec was found; otherwise, false.</returns>
    bool TryGetSingSpec<T>([MaybeNullWhen(false)] out T? spec) where T : ComponentSpec;

    /// <summary>
    /// Get all specs of type T.
    /// </summary>
    /// <typeparam name="T">The type of the specs to get.</typeparam>
    /// <param name="specs">The output parameter that will contain the specs if found.</param>
    /// <returns>True if any specs were found; otherwise, false.</returns>
    bool TryGetSpecs<T>([MaybeNullWhen(false)] out IEnumerable<T>? specs) where T : ComponentSpec;
}

/// <summary>
/// Indicate that this service can modify the specs before returning them.
/// </summary>
public interface ISpecModifier : ISpecServiceFrontRunner
{
    /// <summary>
    /// The priority in which this service will be called. Higher ones are called first.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Modify a single spec before returning it.
    /// </summary>
    /// <typeparam name="T">The type of the spec to modify.</typeparam>
    /// <param name="current">The current spec to be modified. Could have been modified by other services.</param>
    /// <returns>The modified spec.</returns>
    T ModifyGetSingleSpec<T>(T current) where T : ComponentSpec;

    /// <summary>
    /// Modify the specs before returning them.
    /// </summary>
    /// <typeparam name="T">The type of the specs to modify.</typeparam>
    /// <param name="current">The current specs to be modified. Could have been modified by other services.</param>
    /// <returns>The modified specs.</returns>
    IEnumerable<T> ModifyGetSpecs<T>(IEnumerable<T> current) where T : ComponentSpec;
}

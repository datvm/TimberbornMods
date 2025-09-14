namespace ModdableTimberborn.Common;

/// <summary>
/// A modifier that can modify a moddable value.
/// </summary>
public interface IModdableModifier
{
    /// <summary>
    /// A unique identifier for this modifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The priority of this modifier. Modifiers with lower value are applied first.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Whether this modifier is disabled. Disabled modifiers are ignored.
    /// </summary>
    bool Disabled { get; }

    /// <summary>
    /// An event to signal that the modifier has changed and the value should be recalculated.
    /// </summary>
    event Action? OnChanged;
}

/// <summary>
/// A modifier that can modify a moddable value of type <typeparamref name="TValue"/>.
/// </summary>
public interface IModdableModifier<TModdableValue, TValue> : IModdableModifier
    where TModdableValue : IModdableValue<TValue>
{

    /// <summary>
    /// Modifies the given value.
    /// </summary>
    /// <param name="value">The current and original value</param>
    /// <returns>True to short-circuit further modifiers, false to continue.</returns>
    bool Modify(TModdableValue value);
}
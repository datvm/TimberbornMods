namespace TimberUi.Services;

/// <summary>
/// This entity fragment should be reordered based on its Order
/// </summary>
public interface IEntityFragmentOrder
{

    /// <summary>
    /// The order it should be showed. Negative are guaranteed to be on the top while positive are guaranteed to be on the bottom
    /// versus non-<see cref="IEntityFragmentOrder"/> fragments.
    /// </summary>
    /// <remarks>A value of 0 does nothing.</remarks>
    int Order { get; }
    VisualElement Fragment { get; }
}

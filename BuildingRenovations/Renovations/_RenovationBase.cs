namespace BuildingRenovations.Renovations;

/// <summary>
/// Effect logic for one renovation type. Display/cost/time come from <see cref="Spec"/>,
/// which is assigned by <see cref="RenovationRegistry"/> on load.
/// </summary>
public abstract class RenovationBase
{
    public abstract string Id { get; }

    public RenovationSpec Spec { get; internal set; } = null!;

    public string Name => Spec.Title.Value;

    /// <summary>
    /// Hard filter: if false, the entry is hidden unless "Show unavailable renovations" is on
    /// (wrong building type, missing components, never applicable).
    /// </summary>
    public abstract bool CanRenovate(BuildingRenovationComponent building);

    /// <summary>
    /// Soft unavailability: non-null means the entry still always appears (grayed); click shows
    /// this reason. Null means it can be started (subject to building job state).
    /// Already-active is handled by <see cref="BuildingRenovationComponent"/>.
    /// </summary>
    public virtual string? GetUnavailableReason(BuildingRenovationComponent building) => null;

    /// <summary>Optional extra text under the main description in the shared detail panel.</summary>
    public virtual string? GetExtraDescription(BuildingRenovationComponent building) => null;

    /// <summary>
    /// Apply the renovation effect.
    /// <paramref name="isLoad"/> is true when re-applying after save load (already completed earlier).
    /// </summary>
    public abstract void OnCompleted(BuildingRenovationComponent building, bool isLoad);
}

namespace BuildingRenovations.Renovations;

/// <summary>
/// Timed active renovations: stay in ActiveRenovations until
/// <see cref="ExpirableRenovationComponent"/> counts down the duration.
/// Default duration is <c>Spec.Parameters[0]</c> (days). Override <see cref="GetDurationDays"/> if needed.
/// </summary>
public abstract class ExpirableRenovationBase : RenovationBase
{
    /// <remarks>Default: first parameter of the renovation spec (days).</remarks>
    public virtual float GetDurationDays(BuildingRenovationComponent building)
    {
        if (Spec.Parameters.Length == 0)
        {
            throw new InvalidOperationException(
                $"Expirable renovation '{Id}' has no Parameters[0] duration. Override {nameof(GetDurationDays)} or set Parameters.");
        }

        return Spec.Parameters[0];
    }

    public sealed override void OnCompleted(BuildingRenovationComponent building, bool isLoad)
    {
        OnActivated(building, isLoad);

        var expiry = building.Expirable;
        if (isLoad)
        {
            expiry.ResumeIfNeeded(this);
        }
        else
        {
            expiry.Track(this);
        }
    }

    /// <summary>Apply the timed effect (buff, flag, etc.). Called for fresh complete and load resume.</summary>
    protected virtual void OnActivated(BuildingRenovationComponent building, bool isLoad) { }

    /// <summary>Called when the timed effect ends (active flag already cleared).</summary>
    public virtual void OnExpired(BuildingRenovationComponent building) { }
}

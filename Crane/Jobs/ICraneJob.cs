namespace Crane.Jobs;

public interface ICraneJob : IPrioritizable
{

    bool IsForCrane(CraneComponent crane);

    bool IsAvailable { get; }

    event EventHandler? AvailabilityChanged;

    event EventHandler<PriorityChangedEventArgs>? PriorityChanged;

    /// <summary>Advance this job by <paramref name="hours"/> (caller applies speed multipliers).</summary>
    void ProgressJob(CraneComponent crane, float hours);
}

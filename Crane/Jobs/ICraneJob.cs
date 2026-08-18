namespace Crane.Jobs;

public interface ICraneJob : IPrioritizable
{

    bool IsForCrane(CraneComponent crane);

    bool IsAvailable { get; }

    float Progress { get; }

    event EventHandler? AvailabilityChanged;

    event EventHandler<PriorityChangedEventArgs>? PriorityChanged;

    /// <summary>Advance this job by <paramref name="hours"/> (caller applies speed multipliers).</summary>
    void ProgressJob(CraneComponent crane, float hours);

    string JobNameLoc { get; }
}

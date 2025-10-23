namespace ScientificProjects.Services;

public interface IBaseScientificProjectListener
{
    FrozenSet<string> ListenerIds { get; }
    void OnListenerLoaded(IReadOnlyList<ScientificProjectInfo> activeProjects);
}

public interface IScientificProjectUnlockListener : IBaseScientificProjectListener
{
    FrozenSet<string> UnlockListenerIds { get; }
    void OnProjectUnlocked(ScientificProjectSpec project, IReadOnlyList<ScientificProjectInfo> activeProjects);    
}

public interface IScientificProjectDailyListener : IBaseScientificProjectListener
{
    void OnDailyPaymentResolved(IReadOnlyList<ScientificProjectInfo> activeProjects);
}

public interface IScientificProjectUpgradeListener : IScientificProjectUnlockListener, IScientificProjectDailyListener
{
}

public abstract class DefaultProjectUpgradeListener : IScientificProjectUpgradeListener, ILoadableSingleton
{
    public FrozenSet<string> ListenerIds { get; protected set; } = [];

    public abstract FrozenSet<string> UnlockListenerIds { get; }
    public abstract FrozenSet<string> DailyListenerIds { get; }

    public virtual void Load()
    {
        ListenerIds = [.. UnlockListenerIds, .. DailyListenerIds];
    }

    protected abstract void ProcessActiveProjects(IReadOnlyList<ScientificProjectInfo> activeProjects, ScientificProjectSpec? newUnlock, ActiveProjectsSource source);

    public void OnDailyPaymentResolved(IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        ProcessActiveProjects(activeProjects, null, ActiveProjectsSource.DailyPayment);
    }

    public void OnListenerLoaded(IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        ProcessActiveProjects(activeProjects, null, ActiveProjectsSource.Load);
    }

    public void OnProjectUnlocked(ScientificProjectSpec project, IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        ProcessActiveProjects(activeProjects, project, ActiveProjectsSource.Unlock);
    }

    public enum ActiveProjectsSource
    {
        Load,
        DailyPayment,
        Unlock
    }

}
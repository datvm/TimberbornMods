namespace ScientificProjects.Services;

public class ScientificProjectStateListener(
    ScientificProjectUnlockService unlocks,
    ScientificProjectDailyService daily,
    ScientificProjectService scientificProjectService,
    IEnumerable<IBaseScientificProjectListener> listeners
) : BaseProjectUpgradeWithDailyListener(unlocks, daily)
{
    readonly ImmutableArray<IBaseScientificProjectListener> listeners = [.. listeners];
    readonly ImmutableArray<IScientificProjectUnlockListener> unlockListeners = [.. listeners.OfType<IScientificProjectUnlockListener>()];
    readonly ImmutableArray<IScientificProjectDailyListener> dailyListeners = [.. listeners.OfType<IScientificProjectDailyListener>()];

    FrozenDictionary<string, ImmutableArray<IScientificProjectUnlockListener>> unlockListenersByIds = FrozenDictionary<string, ImmutableArray<IScientificProjectUnlockListener>>.Empty;
    FrozenDictionary<string, ImmutableArray<IScientificProjectDailyListener>> dailyListenersByIds = FrozenDictionary<string, ImmutableArray<IScientificProjectDailyListener>>.Empty;

    public override void Load()
    {
        base.Load();

        unlockListenersByIds = Process(unlockListeners, l => l.UnlockListenerIds);
        dailyListenersByIds = Process(dailyListeners, l => l.ListenerIds);

        ProcessLoadedProjects();
    }

    void ProcessLoadedProjects()
    {
        var listenersByIds = Process(listeners, l => l.ListenerIds);

        var projectsByListeners = GetActiveProjects(listenersByIds);
        foreach (var listener in listeners)
        {
            listener.OnListenerLoaded(GetOrEmpty(projectsByListeners, listener));
        }
    }

    protected override void OnDailyPaymentResolved()
    {
        var projectsByListeners = GetActiveProjects(dailyListenersByIds);

        foreach (var listener in dailyListeners)
        {
            listener.OnDailyPaymentResolved(GetOrEmpty(projectsByListeners, listener));
        }
    }

    protected override void OnProjectUnlocked(ScientificProjectSpec spec)
    {
        if (unlockListenersByIds.TryGetValue(spec.Id, out var listeners))
        {            
            foreach (var listener in listeners)
            {
                var activeProjects = scientificProjectService.GetActiveProjects(listener.ListenerIds).ToArray();
                listener.OnProjectUnlocked(spec, activeProjects);
            }
        }
    }

    static FrozenDictionary<string, ImmutableArray<T>> Process<T>(IEnumerable<T> listeners, Func<T, IEnumerable<string>> idSelector)
        where T : class
        => listeners
            .SelectMany(l => idSelector(l).Select(id => (id, l)))
            .GroupBy(t => t.id, t => t.l)
            .ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray());

    Dictionary<T, List<ScientificProjectInfo>> GetActiveProjects<T>(
        FrozenDictionary<string, ImmutableArray<T>> listenersByIds
    )
        where T : class
    {
        Dictionary<T, List<ScientificProjectInfo>> projectsByListeners = [];
        foreach (var (id, project) in scientificProjectService.ActiveProjects)
        {
            if (!listenersByIds.TryGetValue(id, out var listeners)) { continue; }
            foreach (var listener in listeners)
            {
                if (!projectsByListeners.TryGetValue(listener, out var list))
                {
                    projectsByListeners[listener] = list = [];
                }
                list.Add(project);
            }
        }
        return projectsByListeners;
    }

    static List<ScientificProjectInfo> GetOrEmpty<T>(Dictionary<T, List<ScientificProjectInfo>> dict, T key)
        => dict.TryGetValue(key, out var list) ? list : [];

}

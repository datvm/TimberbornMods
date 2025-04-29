namespace TimberQuests;

public class TimberQuestsUpdater(
    MSettings s,
    ISingletonLoader loader
) : ILoadableSingleton, ITickableSingleton, ISaveableSingleton
{
    static readonly PropertyKey<int> CurrentTickKey = new("UpdaterCurrentTick");

    int ticksPerUpdate;
    int currTick;

    public event Action<int> OnQuestUpdated = delegate { };
    public event Action<int> OnQuestUrgentUpdated = delegate { };

    readonly HashSet<ITimberQuestUpdater> updaters = [];
    readonly HashSet<ITimberQuestUrgentUpdater> urgentUpdaters = [];

    public void Register(ITimberQuestUpdater updater)
    {
        updaters.Add(updater);
    }

    public void Register(ITimberQuestUrgentUpdater updater)
    {
        urgentUpdaters.Add(updater);
    }

    public void Unregister(ITimberQuestUpdater updater)
    {
        updaters.Remove(updater);
    }

    public void Unregister(ITimberQuestUrgentUpdater updater)
    {
        urgentUpdaters.Remove(updater);
    }

    public void Load()
    {
        ticksPerUpdate = s.UpdateFreq.Value;
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (loader.TryGetSingleton(TimberQuestsUtils.SaveKey, out var s)
            && s.Has(CurrentTickKey))
        {
            currTick = s.Get(CurrentTickKey);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(TimberQuestsUtils.SaveKey);
        s.Set(CurrentTickKey, currTick);
    }

    public void Tick()
    {
        ++currTick;

        if (urgentUpdaters.Count > 0)
        {
            foreach (var u in urgentUpdaters)
            {
                u.UpdateQuestUrgent(currTick);
            }
            OnQuestUrgentUpdated(currTick);
        }
        
        if (currTick >= ticksPerUpdate)
        {
            if (updaters.Count > 0)
            {
                foreach (var u in updaters)
                {
                    u.UpdateQuest(currTick);
                }
                OnQuestUpdated(currTick);
            }

            currTick = 0;
        }
    }

}

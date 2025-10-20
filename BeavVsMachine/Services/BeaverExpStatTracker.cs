namespace BeavVsMachine.Services;

public partial class BeaverExpStatTracker(
    IDayNightCycle dayNightCycle,
    ISingletonLoader loader
) : ITickableSingleton, ILoadableSingleton, ISaveableSingleton
{
    public const float ChildXPPercent = .5f / 7; // 50% of adult XP over 7 days
    public const float ChildXPWithBookPercent = .9f / 7; // 90% of adult XP over 7 days with book
    public const float BookKnowledgeBoost = .01f;

    public const float FitnessBuildingMultiplier = 1.5f;

    static readonly SingletonKey SaveKey = new(nameof(BeaverExpStatTracker));
    static readonly PropertyKey<int> CurrentDayKey = new("CurrentDay");

    int currDay = -1;

    readonly Dictionary<BeaverExpComponent, ExpComponents> adults = [];
    readonly Dictionary<BeaverExpComponent, ExpComponents> children = [];

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }
        if (s.Has(CurrentDayKey))
        {
            currDay = s.Get(CurrentDayKey);
        }
    }

    public void RegisterBeaver(BeaverExpComponent exp)
    {
        bool isAdult = exp.GetComponentFast<AdultSpec>();

        ExpComponents comps = new(exp);
        (isAdult ? adults : children)[exp] = comps;
    }

    public void UnregisterBeaver(BeaverExpComponent exp)
    {
        if (!adults.Remove(exp, out var comp))
        {
            children.Remove(exp, out comp);
        }

        comp?.Dispose();
    }

    public void Tick()
    {
        var currDay = dayNightCycle.DayNumber;

        if (currDay != this.currDay)
        {
            this.currDay = currDay;
            UpdateNewDay();
        }

        var timePassed = dayNightCycle.FixedDeltaTimeInHours;

        foreach (var b in adults.Values)
        {
            TickFitness(b, timePassed);
            if (b.IsInsideWorkplace)
            {
                b.Exp.AddPendingWorkExp(timePassed);
            }
        }

        foreach (var b in children.Values)
        {
            TickFitness(b, timePassed);
        }
    }

    public bool HasBook(BeaverExpComponent exp) 
        => (adults.TryGetValue(exp, out var comp) || children.TryGetValue(exp, out comp)) && comp.HasBook;

    void UpdateNewDay()
    {
        var topXP = 0f;
        List<ExpComponents> beaversWithBook = [];
        foreach (var b in adults.Values)
        {
            var xp = b.Exp.UpdateDayWorkExp();
            b.Fitness.UpdateDayFitness();

            if (xp > topXP)
            {
                topXP = xp;
            }

            if (b.HasBook)
            {
                beaversWithBook.Add(b);
            }
        }

        foreach (var b in children.Values)
        {
            b.Fitness.UpdateDayFitness();

            var hasBook = b.HasBook;
            b.Exp.AddExperience(topXP * (hasBook ? ChildXPWithBookPercent : ChildXPPercent));

            if (hasBook)
            {
                beaversWithBook.Add(b);
            }
        }

        // Give book bonus
        if (beaversWithBook.Count > 0)
        {
            var extraXP = BookKnowledgeBoost * topXP;
            TimberUiUtils.LogVerbose(() => $"Giving {beaversWithBook.Count} beavers with book extra XP: {extraXP} (topXP: {topXP})");

            foreach (var b in beaversWithBook)
            {
                b.Exp.AddExperience(extraXP);
            }
        }        
    }

    void TickFitness(ExpComponents exp, float timePassed)
    {
        if (!exp.IsInside)
        {
            exp.Fitness.AddTodayFitness(timePassed);
        }
        else if (exp.IsInsideFitnessBuilding)
        {
            exp.Fitness.AddTodayFitness(timePassed * FitnessBuildingMultiplier);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(CurrentDayKey, currDay);
    }
}

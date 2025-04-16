global using Timberborn.TimeSystem;

namespace LunchBreak.Services;

public class LunchBreakManager(ISingletonLoader loader, IDayNightCycle cycle) : ILoadableSingleton, IUnloadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new("LunchBreak");
    static readonly PropertyKey<Vector2Int> LunchBreakTimeKey = new("LunchBreakTime");

    public static LunchBreakManager? Instance { get; private set; }

    public Vector2Int LunchBreakTime { get; set; } = new(7, 7);
    
    public bool IsLunchBreakTime
    {
        get
        {
            var x = LunchBreakTime.x;
            var y = LunchBreakTime.y;
            if (x == y) { return false; }

            var curr = cycle.HoursPassedToday;
            return curr >= x && curr < y;
        }
    }

    public void Load()
    {
        LoadSavedData();

        Instance = this;
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(LunchBreakTimeKey))
        {
            LunchBreakTime = s.Get(LunchBreakTimeKey);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(LunchBreakTimeKey, LunchBreakTime);
    }

    public void Unload()
    {
        Instance = null;
    }
}

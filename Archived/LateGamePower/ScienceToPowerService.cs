global using Timberborn.Persistence;
global using Timberborn.ScienceSystem;
global using Timberborn.TickSystem;
global using Timberborn.TimeSystem;

namespace LateGamePower;

public class ScienceToPowerService : ITickableSingleton, ISaveableSingleton, ILoadableSingleton
{
    public static ScienceToPowerService? Instance { get; private set; }

    static readonly SingletonKey S2pKey = new("ScienceToPower");
    static readonly PropertyKey<int>
        targetMultiplicationKey = new("TargetMultiplication"),
        currentMultiplicationKey = new("CurrentMultiplication"),
        lastPaymentDayKey = new("LastPaymentDay");
    static readonly PropertyKey<float>
        lastPaymentHourKey = new("LastPaymentHour");

    readonly ISingletonLoader loader;
    readonly IDayNightCycle time;
    readonly ScienceService sciences;
    readonly ModSettings settings;

    public ScienceToPowerService(ISingletonLoader loader, IDayNightCycle time, ScienceService sciences, ModSettings settings)
    {
        Instance = this;

        this.loader = loader;
        this.time = time;
        this.sciences = sciences;
        this.settings = settings;
    }

    public int CurrentMultiplication { get; private set; } = 1;
    public bool NotEnoughScience { get; private set; }

    (int Day, float Hour) lastPayment = default;

    int targetMultiplication = 1;
    public int TargetMultiplication
    {
        get
        {
            EnsureMultiplicationRange();
            return targetMultiplication;
        }

        set
        {
            targetMultiplication = value;
            EnsureMultiplicationRange();
        }
    }

    void EnsureMultiplicationRange()
    {
        var max = settings.MaxMultiplier;
        if (targetMultiplication > max) { targetMultiplication = max; }
        if (targetMultiplication < 1) { targetMultiplication = 1; }
    }

    public void Tick()
    {
        if (CurrentMultiplication == 1 && TargetMultiplication == 1) { return; }

        var day = time.DayNumber;
        var hour = time.HoursPassedToday;

        var hourPassed = hour - lastPayment.Hour;
        if (day > lastPayment.Day)
        {
            hourPassed += time.SecondsToHours(time.ConfiguredDayLengthInSeconds) * (day - lastPayment.Day);
        }
        if (hourPassed < 1) { return; }

        if (TargetMultiplication <= 1)
        {
            CurrentMultiplication = 1;
            return;
        }

        var cost = CalculateScienceCost(TargetMultiplication);
        if (sciences.SciencePoints < cost)
        {
            NotEnoughScience = true;
            CurrentMultiplication = 1;
            return;
        }
        else
        {
            NotEnoughScience = false;
            sciences.SubtractPoints(cost);
            CurrentMultiplication = TargetMultiplication;
            lastPayment = (day, hour);
        }
    }

    public int CalculateScienceCost(int multiplication)
    {
        if (multiplication < 1) { return 0; }

        return CustomFibo(multiplication - 2, settings.BaseCost);
    }

    static int CustomFibo(int n, int startingNum)
    {
        if (n == 0 || startingNum == 0) return startingNum;
        if (n == 1) return startingNum * 2;

        var prev2 = startingNum;
        var prev1 = startingNum * 2;
        var current = 0;

        for (int i = 2; i <= n; i++)
        {
            current = prev1 + prev2;
            prev2 = prev1;
            prev1 = current;
        }

        return current;
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var set = singletonSaver.GetSingleton(S2pKey);

        set.Set(targetMultiplicationKey, TargetMultiplication);
        set.Set(currentMultiplicationKey, CurrentMultiplication);
        set.Set(lastPaymentDayKey, lastPayment.Day);
        set.Set(lastPaymentHourKey, lastPayment.Hour);
    }

    public void Load()
    {
        if (!loader.HasSingleton(S2pKey)) { return; }

        var set = loader.GetSingleton(S2pKey);
        TargetMultiplication = Math.Max(settings.MaxMultiplier, set.Get(targetMultiplicationKey));
        CurrentMultiplication = set.Get(currentMultiplicationKey);
        lastPayment = (set.Get(lastPaymentDayKey), set.Get(lastPaymentHourKey));
    }
}

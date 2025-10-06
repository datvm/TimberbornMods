namespace ModdableTimberborn.Helpers;

public static class ModdableTimberbornUtils
{

    public static ConfigurationContext CurrentContext { get; internal set; } = ConfigurationContext.Bootstrapper;

    public static void LogVerbose(Func<string> msg)
    {
        TimberUiUtils.LogVerbose(() => $"[{nameof(ModdableTimberborn)}] {msg()}");
    }

    public static void AddSlotsToPrefab(BaseComponent component, int minimumSlots)
    {
        var patrolling = component.GetComponentFast<PatrollingSlotInitializerSpec>();
        if (patrolling)
        {
            patrolling._patrollingSlots.KeepAddingUntil(minimumSlots);
        }

        var transformSlot = component.GetComponentFast<TransformSlotInitializerSpec>();
        if (transformSlot)
        {
            transformSlot._slots.KeepAddingUntil(minimumSlots);
        }
    }

    public static void KeepAddingUntil<T>(this List<T> list, int toMimnimum)
    {
        var currCount = list.Count;
        var addingAmount = toMimnimum - currCount;

        for (int i = 0; i < addingAmount; i++)
        {
            list.Add(list[i % currCount]);
        }
    }

}

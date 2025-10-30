namespace ModdableTimberborn.Helpers;

public static class ModdableTimberbornUtils
{

    public static ConfigurationContext CurrentContext { get; internal set; } = ConfigurationContext.Bootstrapper;

    public static void LogVerbose(Func<string> msg)
    {
        TimberUiUtils.LogVerbose(() => $"[{nameof(ModdableTimberborn)}] {msg()}");
    }

    public static void AddSlotsToPrefab(Blueprint blueprint, int minimumSlots)
    {
        ImmutableArray<PatrollingSlotSpec>? patrollingSlots = null;
        ImmutableArray<TransformSlotSpec>? transformSlots = null;

        var patrolling = blueprint.GetSpec<PatrollingSlotInitializerSpec>();
        if (patrolling is not null)
        {
            patrollingSlots = [.. KeepAddingUntil(patrolling.PatrollingSlots, minimumSlots)];
        }

        var transformSlot = blueprint.GetSpec<TransformSlotInitializerSpec>();
        if (transformSlot is not null)
        {
            transformSlots = [.. KeepAddingUntil(transformSlot.Slots, minimumSlots)];
        }

        if (patrollingSlots is null && transformSlots is null) { return; }


    }

    public static IEnumerable<T> KeepAddingUntil<T>(this IEnumerable<T> list, int toMimnimum)
    {
        if (toMimnimum == 0) { yield break; }

        var count = 0;
        while (true)
        {
            foreach (var item in list)
            {
                yield return item;
                count++;

                if (count >= toMimnimum)
                {
                    yield break;
                }
            }
        }
    }

}

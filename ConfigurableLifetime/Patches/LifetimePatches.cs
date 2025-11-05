namespace ConfigurableLifetime.Patches;

[HarmonyPatch]
public static class LifetimePatches
{
    static readonly ConditionalWeakTable<LifeProgressor, object> characterCache = [];

    [HarmonyPrefix, HarmonyPatch(typeof(LifeProgressor), nameof(LifeProgressor.IncreaseLifeProgress))]
    public static bool StopLifeProgress(LifeProgressor __instance)
    {
        var mul = MSettings.Instance?.BeaverLifeMul.Value;
        if (mul is null or 1f || IsChild(__instance))
        {
            return true;
        }

        __instance.LifeProgress +=
            __instance._lifeProgressIncreasePerTick
            * mul.Value
            / __instance._bonusManager.Multiplier(LifeProgressor.LifeExpectancyBonusId);

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Deteriorable), nameof(Deteriorable.Tick))]
    public static void MultiplyDeteriorableLifeProgress(Deteriorable __instance)
    {
        var mul = MSettings.Instance?.BotLifeMul.Value;
        if (__instance._currentDeterioration <= 0f || mul is null or 1f) { return; }

        // Give the difference between this and the supposed _currentDeterioration -= _fixedDeltaTimeInDays;
        var willBeValue = __instance._currentDeterioration - __instance._fixedDeltaTimeInDays;
        var shouldBeValue = __instance._currentDeterioration - __instance._fixedDeltaTimeInDays * mul.Value;
        var diff = willBeValue - shouldBeValue;
        __instance._currentDeterioration -= diff;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Child), nameof(Child.GrowthProgressPerTick))]
    public static void MultiplyChildGrowth(ref float __result)
    {
        var mul = MSettings.Instance?.ChildhoodDaysMul.Value;
        if (mul is null or 1f) { return; }

        __result *= mul.Value;
    }

    static bool IsChild(LifeProgressor lifeProgressor)
    {
        if (!characterCache.TryGetValue(lifeProgressor, out var value))
        {
            var child = lifeProgressor.GetComponent<Child>();
            characterCache.Add(lifeProgressor, (bool?)child);
        }

        return (bool?)value == true;
    }

}

public enum CharacterType
{
    Adult,
    Child,
    Bot
}
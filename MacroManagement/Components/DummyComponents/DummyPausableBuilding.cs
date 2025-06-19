namespace MacroManagement.Components.DummyComponents;

public class DummyPausableBuilding : PausableBuilding, IDummyComponent<DummyPausableBuilding, PausableBuilding>
{
    public MMComponent MMComponent { get; set; } = null!;

    public void Init(PausableBuilding original)
    {
        Paused = original.Paused;
    }

    public new void Pause()
    {
        this.Proxy(q => q.Pause());
        Paused = true;
    }

    public new void Resume()
    {
        this.Proxy(q => q.Resume());
        Paused = false;
    }

    public new bool IsPausable()
    {
        var result = false;

        this.Proxy(q =>
        {
            if (q.IsPausable())
            {
                result = true;
                return true;
            }

            return false;
        });

        return result;
    }

}

[HarmonyPatch(typeof(PausableBuilding))]
public static class PausableBuildingRedirect
{

    [HarmonyPrefix, HarmonyPatch(nameof(PausableBuilding.Awake))]
    public static bool BypassAwake(PausableBuilding __instance)
        => __instance.PatchBypass<DummyPausableBuilding, PausableBuilding>();

    [HarmonyPrefix, HarmonyPatch(nameof(PausableBuilding.Start))]
    public static bool BypassStart(PausableBuilding __instance)
        => __instance.PatchBypass<DummyPausableBuilding, PausableBuilding>();


    [HarmonyPrefix, HarmonyPatch(nameof(PausableBuilding.Pause))]
    public static bool RedirectPause(PausableBuilding __instance)
        => __instance.PatchRedirect<DummyPausableBuilding, PausableBuilding>(q => q.Pause());

    [HarmonyPrefix, HarmonyPatch(nameof(PausableBuilding.Resume))]
    public static bool RedirectResume(PausableBuilding __instance)
        => __instance.PatchRedirect<DummyPausableBuilding, PausableBuilding>(q => q.Resume());

    [HarmonyPrefix, HarmonyPatch(nameof(PausableBuilding.IsPausable))]
    public static bool RedirectIsPausable(PausableBuilding __instance, ref bool __result)
        => __instance.PatchRedirect<DummyPausableBuilding, PausableBuilding, bool>(q => q.IsPausable(), ref __result);


}

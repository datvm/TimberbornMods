namespace MacroManagement.Components.DummyComponents;

public class DummyWorkplacePriority : WorkplacePriority, IDummyComponent<DummyWorkplacePriority, WorkplacePriority>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
#nullable enable

    public void Init(WorkplacePriority original)
    {
        Priority = original.Priority;
    }

    public new void SetPriority(Timberborn.PrioritySystem.Priority priority)
    {
        this.Proxy(q => q.SetPriority(priority));
        Priority = priority;
    }
}

[HarmonyPatch(typeof(WorkplacePriority))]
public static class WorkplacePriorityRedirect
{

    [HarmonyPrefix, HarmonyPatch(nameof(WorkplacePriority.Awake))]
    public static bool BypassAwake(WorkplacePriority __instance)
        => __instance.PatchBypass<DummyWorkplacePriority, WorkplacePriority>();

    [HarmonyPrefix, HarmonyPatch(nameof(WorkplacePriority.SetPriority))]
    public static bool RedirectSetPriority(WorkplacePriority __instance, Timberborn.PrioritySystem.Priority priority)
        => __instance.PatchRedirect<DummyWorkplacePriority, WorkplacePriority>(q => q.SetPriority(priority));
}

namespace MacroManagement.Components.DummyComponents;

public class DummyWorkplaceWorkerType : WorkplaceWorkerType, IDummyComponent<DummyWorkplaceWorkerType, WorkplaceWorkerType>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
#nullable enable

    public void Init(WorkplaceWorkerType original)
    {
        WorkerType = original.WorkerType;
    }

    public new void SetWorkerType(string workerType)
    {
        this.Proxy(q => q.SetWorkerType(workerType));
        WorkerType = workerType;
    }
}

[HarmonyPatch(typeof(WorkplaceWorkerType))]
public static class WorkplaceWorkerTypeRedirect
{
    [HarmonyPrefix, HarmonyPatch(nameof(WorkplaceWorkerType.Awake))]
    public static bool BypassAwake(WorkplaceWorkerType __instance)
        => __instance.PatchBypass<DummyWorkplaceWorkerType, WorkplaceWorkerType>();

    [HarmonyPrefix, HarmonyPatch(nameof(WorkplaceWorkerType.SetWorkerType))]
    public static bool RedirectSetWorkerType(WorkplaceWorkerType __instance, string workerType)
        => __instance.PatchRedirect<DummyWorkplaceWorkerType, WorkplaceWorkerType>(q => q.SetWorkerType(workerType));
}
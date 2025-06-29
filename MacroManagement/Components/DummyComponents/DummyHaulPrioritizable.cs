namespace MacroManagement.Components.DummyComponents;

public class DummyHaulPrioritizable : HaulPrioritizable, IDummyComponent<DummyHaulPrioritizable, HaulPrioritizable>, IPersistentEntity, IFinishedStateListener
{
#nullable disable
    public MMComponent MMComponent { get; set; }
#nullable enable

    public new bool Prioritized
    {
        get;
        set
        {
            field = value;
            this.Proxy(q =>
            {
                q.Prioritized = value;
                return false;
            });
        }
    }

    public new bool PrioritizationVisible => this.ProxyFirstOrDefault(q => q.PrioritizationVisible);

    public void Init(HaulPrioritizable original)
    {
        Prioritized = original.Prioritized;
    }

    void IPersistentEntity.Save(IEntitySaver entitySaver) { }
    void IPersistentEntity.Load(IEntityLoader entityLoader) { }
    void IFinishedStateListener.OnEnterFinishedState() { }
    void IFinishedStateListener.OnExitFinishedState() { }

}

[HarmonyPatch(typeof(HaulPrioritizable))]
public static class HaulPrioritizableRedirect
{
    [HarmonyPrefix, HarmonyPatch(nameof(HaulPrioritizable.Awake))]
    public static bool BypassAwake(HaulPrioritizable __instance)
        => __instance.PatchBypass<DummyHaulPrioritizable, HaulPrioritizable>();

    [HarmonyPrefix, HarmonyPatch(nameof(HaulPrioritizable.Prioritized), MethodType.Getter)]
    public static bool RedirectGetPrioritized(HaulPrioritizable __instance, ref bool __result)
        => __instance.PatchRedirect<DummyHaulPrioritizable, HaulPrioritizable, bool>(q => q.Prioritized, ref __result);

    [HarmonyPrefix, HarmonyPatch(nameof(HaulPrioritizable.Prioritized), MethodType.Setter)]
    public static bool RedirectSetPrioritized(HaulPrioritizable __instance, bool value)
        => __instance.PatchRedirect<DummyHaulPrioritizable, HaulPrioritizable>(q => q.Prioritized = value);

    [HarmonyPrefix, HarmonyPatch(nameof(HaulPrioritizable.PrioritizationVisible), MethodType.Getter)]
    public static bool RedirectGetPrioritizationVisible(HaulPrioritizable __instance, ref bool __result)
        => __instance.PatchRedirect<DummyHaulPrioritizable, HaulPrioritizable, bool>(q => q.PrioritizationVisible, ref __result);

}
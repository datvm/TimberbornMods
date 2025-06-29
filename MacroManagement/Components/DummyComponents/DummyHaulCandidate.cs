namespace MacroManagement.Components.DummyComponents;

public class DummyHaulCandidate : HaulCandidate, IDummyComponent<DummyHaulCandidate, HaulCandidate>, IFinishedStateListener
{

#nullable disable
    public MMComponent MMComponent { get; set; }
#nullable enable

    public void Init(HaulCandidate original)
    {        
    }

    public new bool IsInHaulingCenterRange() => this.ProxyFirstOrDefault(q => q.IsInHaulingCenterRange);

    public new void OnEnterFinishedState() { }
    public new void OnExitFinishedState() { }

}

[HarmonyPatch(typeof(HaulCandidate))]
public static class HaulCandidateRedirect
{

    [HarmonyPrefix, HarmonyPatch(nameof(HaulCandidate.Awake))]
    public static bool BypassAwake(HaulCandidate __instance)
        => __instance.PatchBypass<DummyHaulCandidate, HaulCandidate>();

    [HarmonyPrefix, HarmonyPatch(nameof(HaulCandidate.IsInHaulingCenterRange), MethodType.Getter)]
    public static bool RedirectIsInHaulingCenterRange(HaulCandidate __instance, ref bool __result)
    {
        return __instance.PatchRedirect<DummyHaulCandidate, HaulCandidate, bool>(q => q.IsInHaulingCenterRange(), ref __result);
    }

}


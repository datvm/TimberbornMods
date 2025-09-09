
namespace MacroManagement.Components.DummyComponents;

public class DummyDynamite : Dynamite, IWarningDummyComponent<DummyDynamite, Dynamite>
{
#nullable disable
    public ILoc T { get; set; }
    public DialogBoxShower DiaglogBoxShower { get; set; }
    public MMComponent MMComponent { get; set; }
#nullable enable

    public void Init(Dynamite original)
    {
        _blockObject = original._blockObject;
        _blockService = original._blockService;
        _dynamiteSpec = original._dynamiteSpec;
    }

    [Inject]
    public void Inject(ILoc t, DialogBoxShower diag)
    {
        T = t;
        DiaglogBoxShower = diag;
    }

    public new void Detonate()
    {
        this.Confirm("LV.MacM.DetonateWarning", () =>
        {
            this.Proxy(q => q.Detonate());
        });
    }

}

[HarmonyPatch(typeof(Dynamite))]
public static class DynamiteRedirect
{

    [HarmonyPrefix, HarmonyPatch(nameof(Dynamite.Awake))]
    public static bool BypassAwake(Dynamite __instance)
        => __instance.PatchBypass<DummyDynamite, Dynamite>();

    [HarmonyPrefix, HarmonyPatch(nameof(Dynamite.Detonate))]
    public static bool RedirectDetonate(Dynamite __instance)
        => __instance.PatchRedirect<DummyDynamite, Dynamite>(dd => dd.Detonate());

}

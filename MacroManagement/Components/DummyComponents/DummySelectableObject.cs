namespace MacroManagement.Components.DummyComponents;

public class DummySelectableObject : SelectableObject, IDummyComponent<DummySelectableObject, SelectableObject>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
#nullable enable

    public event Action<DummySelectableObject>? OnSelected;
    public event Action<DummySelectableObject>? OnUnselected;

    public void Init(SelectableObject original)
    {
        _cameraTarget = new DummyCameraTarget(original.CameraTargetPosition);
    }

    public new void OnSelect()
    {
        OnSelected?.Invoke(this);
    }

    public new void OnUnselect()
    {
        OnUnselected?.Invoke(this);
    }

}
[HarmonyPatch(typeof(SelectableObject))]
public static class SelectableObjectRedirect
{
    [HarmonyPrefix, HarmonyPatch(nameof(SelectableObject.Awake))]
    public static bool BypassAwake(SelectableObject __instance)
        => __instance.PatchBypass<DummySelectableObject, SelectableObject>();

    [HarmonyPrefix, HarmonyPatch(nameof(SelectableObject.OnSelect))]
    public static bool RedirectOnSelect(SelectableObject __instance)
        => __instance.PatchRedirect<DummySelectableObject, SelectableObject>(q => q.OnSelect());

    [HarmonyPrefix, HarmonyPatch(nameof(SelectableObject.OnUnselect))]
    public static bool RedirectOnUnselect(SelectableObject __instance)
        => __instance.PatchRedirect<DummySelectableObject, SelectableObject>(q => q.OnUnselect());
}

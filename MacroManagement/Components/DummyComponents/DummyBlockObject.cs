namespace MacroManagement.Components.DummyComponents;

public class DummyBlockObject : BlockObject, IDummyComponent<DummyBlockObject, BlockObject>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
#nullable enable

    public void Init(BlockObject original)
    {
        _blockObjectState = original._blockObjectState;
        PositionedBlocks = new([]);
    }
}

[HarmonyPatch(typeof(BlockObject))]
public static class BlockObjectRedirect
{
    [HarmonyPrefix, HarmonyPatch(nameof(BlockObject.Awake))]
    public static bool BypassAwake(BlockObject __instance)
        => __instance.PatchBypass<DummyBlockObject, BlockObject>();

    [HarmonyPrefix, HarmonyPatch(nameof(BlockObject.Save))]
    public static bool BypassSave(BlockObject __instance)
        => __instance.PatchBypass<DummyBlockObject, BlockObject>();

    [HarmonyPrefix, HarmonyPatch(nameof(BlockObject.Load))]
    public static bool BypassLoad(BlockObject __instance)
        => __instance.PatchBypass<DummyBlockObject, BlockObject>();

    [HarmonyPrefix, HarmonyPatch(nameof(BlockObject.AddToService))]
    public static bool BypassAddToService(BlockObject __instance)
        => __instance.PatchBypass<DummyBlockObject, BlockObject>();

    [HarmonyPrefix, HarmonyPatch(nameof(BlockObject.RemoveFromService))]
    public static bool BypassRemoveFromService(BlockObject __instance)
        => __instance.PatchBypass<DummyBlockObject, BlockObject>();
}
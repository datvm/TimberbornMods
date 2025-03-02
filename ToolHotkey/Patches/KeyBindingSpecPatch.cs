namespace ToolHotkey.Patches;

[HarmonyPatch]
public static class KeyBindingSpecPatch
{

    public static IEnumerable<KeyBindingSpec> AddingSpecs = [];

    [HarmonyPostfix, HarmonyPatch(typeof(KeyBindingSpecService), nameof(KeyBindingSpecService.Load))]
    public static void PatchKeyBindingSpec(KeyBindingSpecService __instance)
    {
        IEnumerable<AdditionalToolSpec> tools = ToolPersistence.LoadTools();
        if (!tools.Any()) { return; }

        tools = tools
            .OrderBy(q => q.Name);

        var counter = 0;
        foreach (var t in tools)
        {
            var display = t.Name;

            __instance._defaultKeyBindingSpecs.Add(new()
            {
                Id = string.Format(ModStarter.ToolKeyId, t.Id),
                GroupId = ModStarter.KeyGroupId,
                Order = counter++,
                LocKey = display,

                PrimaryInputBindingSpec = new(),
                SecondaryInputBindingSpec = new(),
            });
        }
    }

}

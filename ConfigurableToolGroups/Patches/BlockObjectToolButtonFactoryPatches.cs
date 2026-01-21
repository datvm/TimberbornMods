namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(BlockObjectToolButtonFactory))]
public static class BlockObjectToolButtonFactoryPatches
{

    static readonly ConditionalWeakTable<PlaceableBlockObjectSpec, BlockObjectTool> tools = [];

    [HarmonyPrefix, HarmonyPatch(nameof(BlockObjectToolButtonFactory.CreateTool))]
    public static bool CheckForCache(PlaceableBlockObjectSpec template, ref BlockObjectTool __result)
    {
        if (tools.TryGetValue(template, out var cachedTool))
        {
            __result = cachedTool;
            return false;
        }

        return true;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(BlockObjectToolButtonFactory.CreateTool))]
    public static void CacheTool(PlaceableBlockObjectSpec template, BlockObjectTool __result)
    {
        tools.AddOrUpdate(template, __result);
    }

}

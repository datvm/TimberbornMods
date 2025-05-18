namespace Ziporter.Patches;

[HarmonyPatch]
public static class PathPatches
{
    static int? ZiporterGroupId => ZiporterNavGroupService.Instance?.GroupId;

    [HarmonyPrefix, HarmonyPatch(typeof(ZiplineGroupService), nameof(ZiplineGroupService.IsAnyZiplineGroup))]
    public static void AddZiporterGroup(ZiplineGroupService __instance, ref int groupId)
    {
        if (groupId == ZiporterGroupId)
        {
            groupId = __instance.RegularGroupId;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ZiplineGroupService), nameof(ZiplineGroupService.IsRegularEdge))]
    public static void AddZiporterRegularEdge(ZiplineGroupService __instance, ref int fromGroupId, ref int toGroupId)
        => AddZiporterToEdge(__instance, ref fromGroupId, ref toGroupId);

    [HarmonyPrefix, HarmonyPatch(typeof(ZiplineGroupService), nameof(ZiplineGroupService.IsTurnEdge))]
    public static void AddZiporterTurnEdge(ZiplineGroupService __instance, ref int fromGroupId, ref int toGroupId)
        => AddZiporterToEdge(__instance, ref fromGroupId, ref toGroupId);

    static void AddZiporterToEdge(ZiplineGroupService __instance, ref int fromGroupId, ref int toGroupId)
    {
        var id = ZiporterGroupId;
        if (fromGroupId == id)
        {
            fromGroupId = __instance.RegularGroupId;
        }
        if (toGroupId == id)
        {
            toGroupId = __instance.RegularGroupId;
        }
    }
}

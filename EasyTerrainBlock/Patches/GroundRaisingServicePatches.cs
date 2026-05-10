namespace EasyTerrainBlock.Patches;

[HarmonyPatch(typeof(GroundRaisingService))]
public static class GroundRaisingServicePatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(GroundRaisingService.OnEntityDeleted))]
    public static bool OnEntityDeleted(EntityDeletedEvent entityDeletedEvent)
    {
        var raiser = entityDeletedEvent.Entity.GetComponent<GroundRaiser>();

        if (!raiser || !raiser.ShouldRaiseTerrain)
        {
            return false;
        }

        GroundRaisingSupportService.Instance?.ClearObjectsAt(raiser);
        
        return true;
    }

}

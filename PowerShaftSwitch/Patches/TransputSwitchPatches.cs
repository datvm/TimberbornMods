namespace PowerShaftSwitch.Patches;

[HarmonyPatch(typeof(MechanicalNode))]
public static class TransputSwitchPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(NetworkFragmentService), nameof(NetworkFragmentService.Update))] 
    public static bool PatchUIOnPaused(NetworkFragmentService __instance, MechanicalNode mechanicalNode)
    {
        if (!mechanicalNode || mechanicalNode.Graph?.CurrentPower is null)
        {
            __instance._label.text = __instance._loc.T("Status.Buildings.Paused");
            __instance._label.ToggleDisplayStyle(true);
            return false;
        }

        return true;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(MechanicalNode.OnEnterFinishedState))]
    public static void PatchShaftFinishState(MechanicalNode __instance)
    {
        if (!__instance.IsShaft) { return; }

        var sw = __instance.GetComponentFast<TransputSwitchComponent>();
        if (sw)
        {
            sw.OnMechNodeEnterFinishedState();
        }
    }

    [HarmonyPrefix, HarmonyPatch(nameof(MechanicalNode.OnExitFinishedState))]
    public static bool PatchShaftExitState(MechanicalNode __instance)
    {
        if (!__instance.IsShaft) { return true; }

        var sw = __instance.GetComponentFast<TransputSwitchComponent>();
        return sw ? sw.OnMechNodeExitFinishedState() : true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(EntityService), nameof(EntityService.Delete))]
    public static void UnpauseShaftBeforeDeleting(BaseComponent entity, out ImmutableHashSet<PausableBuilding>? __state)
    {
        __state = default;
        var mechNode = entity.GetComponentFast<MechanicalNode>();        
        if (!mechNode || TransputSwitchService.Instance is null) { return; }

        __state = TransputSwitchService.Instance.GetPausedSwitches(mechNode);
        foreach (var b in __state)
        {
            b.Resume();
        }
        
    }

    [HarmonyPostfix, HarmonyPatch(typeof(EntityService), nameof(EntityService.Delete))]
    public static void PauseTheShaftAgain(ImmutableHashSet<PausableBuilding>? __state)
    {
        if (__state is null || __state.Count == 0) { return; }

        foreach (var b in __state)
        {
            b.Pause();
        }
    }

}

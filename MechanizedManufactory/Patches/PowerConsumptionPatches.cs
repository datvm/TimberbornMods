namespace MechanizedManufactory.Patches;

[HarmonyPatch(typeof(WorkplacePowerConsumptionSwitch))]
static class PowerConsumptionPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(WorkplacePowerConsumptionSwitch.UpdatePowerConsumption))]
    public static bool DoNotTurnOffPower(WorkplacePowerConsumptionSwitch __instance)
    {
        if (__instance.GetComponent<MechanizedManufactorySpec>() is null) { return true; }

        __instance.DisableComponent();
        return false;
    }

}

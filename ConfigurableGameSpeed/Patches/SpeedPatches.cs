global using Timberborn.TimeSystemUI;

namespace ConfigurableGameSpeed.Patches;

[HarmonyPatch]
public static class SpeedPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(SpeedControlPanel), nameof(SpeedControlPanel.SetSpeed))]
    public static void PatchSpeed(ref float timeSpeed)
    {
        for (int i = 0; i < MSettings.DefaultSpeeds.Length; i++)
        {
            if (Mathf.Approximately(timeSpeed, MSettings.DefaultSpeeds[i]))
            {
                timeSpeed = MSettings.Speeds[i];
                break;
            }
        }
    }

}

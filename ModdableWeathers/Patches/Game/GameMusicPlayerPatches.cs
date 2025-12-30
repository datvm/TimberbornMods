namespace ModdableWeathers.Patches.Game;

[HarmonyPatch(typeof(GameMusicPlayer))]
public static class GameMusicPlayerPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(GameMusicPlayer.Load))]
    public static void StopDefaultMusic(GameMusicPlayer __instance)
    {
        __instance.StopStandardMusic();
        __instance.StopDroughtMusic();
    }

}

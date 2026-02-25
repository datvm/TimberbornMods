namespace ModdableTimberbornAchievements;

public class MStarter : IModStarter
{
    public const string SteamCategory = "Steam";

    public static bool HasSteam { get; private set; }
    public static bool HasTImprove4Achievements { get; private set; }

    public void StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(ModdableTimberbornAchievements));
        h.PatchAllUncategorized();

        HasSteam = TimberUiUtils.LoadedAssemblyNames.Contains("Timberborn.SteamAchievementSystem");
        HasTImprove4Achievements = TimberUiUtils.LoadedAssemblyNames.Contains("TImprove4Achievements");

        if (HasSteam)
        {
            h.PatchCategory(SteamCategory);
        }
    }

}

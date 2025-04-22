namespace TImprove4Mods.Services;

public class LegacyModWarningService(DialogBoxShower shower) : ILoadableSingleton
{
    public void Load()
    {
        if (ModStarter.HasLegacyMod)
        {
            shower.Create()
                .SetLocalizedMessage("LV.T4Mods.LegacyWarning")
                .Show();
        }
    }
}

namespace ConfigurableFaction.Services;

public class ConflictModChecker(
    ModRepository modRepository,
    DialogBoxShower diag
) : ILoadableSingleton
{

    public void Load()
    {
        if (!modRepository.EnabledMods.Any(q => q.Manifest.Id == ExportToModService.ModId)) { return; }

        diag.Create()
            .SetLocalizedMessage("LV.CFac.ConflictWarning")
            .Show();
    }

}

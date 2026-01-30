namespace XtraPlzNoCrashes;

public class ModVerifier(
    ModRepository modRepository,
    DialogBoxShower diag,
    ILoc t
) : ILoadableSingleton
{

    static readonly FrozenSet<string> ReplacementIds = [
        "MaterialPlzNoCrashes", "MultiYieldersPlzNoCrashes"
    ];

    public void Load()
    {
        foreach (var m in modRepository.EnabledMods)
        {
            if (ReplacementIds.Contains(m.Manifest.Id))
            {
                diag.Create()
                    .SetMessage(t.T("LV.MPNC.ModActive", m.Manifest.Name))
                    .Show();
            }
        }
    }

}

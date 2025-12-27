namespace ModdableWeathers.Components.Replace;

[HasPatch]
[BypassMethods([
    nameof(InitializeEntity),
])]
public class DisableDroughtWaterStrengthModifier() :
    DroughtWaterStrengthModifier(null, null, null, null, null, null, null)
{

    ModdableWaterStrengthModifier modifier = null!;

    [ReplaceMethod]
    public void MAwake()
    {
        modifier = GetComponent<ModdableWaterStrengthModifier>();
    }

    [ReplaceMethod]
    public void MEnable()
    {
        modifier.IgnoreDrought = false;
    }

    [ReplaceMethod]
    public void MDisable()
    {
        modifier.IgnoreDrought = true;
    }

}

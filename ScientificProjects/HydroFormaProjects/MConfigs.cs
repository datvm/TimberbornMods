

namespace HydroFormaProjects;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<DamGateService>()

            .MultiBindSingleton<IPrefabModifier, PrefabModifier>()

            .BindFragment<DamGateFragment>()
        ;
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(HydroFormaProjects)).PatchAll();
    }

}

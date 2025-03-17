namespace ConfigurableHousing;

[Context("MainMenu")]
[Context("Game")]
public class ModSettingConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableHousing)).PatchAll();
    }

}

global using Timberborn.DwellingSystem;
global using Timberborn.Reproduction;

namespace ConfigurableHousing;

[Context("MainMenu")]
public class ModMainMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class ModGameContextConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();

        if (MSettings.AddOtherFaction && MSettings.AddProcreation)
        {
            MultiBind<TemplateModule>().ToProvider(() =>
            {
                TemplateModule.Builder b = new();
                b.AddDecorator<Dwelling, ProcreationHouseSpec>();
                b.AddDecorator<Dwelling, ProcreationHouse>();
                return b.Build();
            }).AsSingleton();
        }
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableHousing)).PatchAll();
    }

}

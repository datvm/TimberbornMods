global using ColorfulBeavers.Graphical;
global using Timberborn.GameDistrictsUI;

namespace ColorfulBeavers;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public class FragmentsProvider(BeaverColorFragment color) : EntityPanelFragmentProvider([color]);

    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<Character, BeaverColorComponent>();
            b.AddDecorator<CitizenDistrictTintChanger, CitizenDistrictTintResettter>();
            return b.Build();
        }).AsSingleton();

        this.BindFragments<FragmentsProvider>();
    }

}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        BeaverColorSettings.Instance.Load();
    }

}
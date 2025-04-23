global using ConfigurableWorkplace.Components;
global using ConfigurableWorkplace.UI;

namespace ConfigurableWorkplace;

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
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();

        if (MSettings.BuildingShift)
        {
            this.BindTemplateModule()
                .AddDecorator<WorkplaceWorkingHours, WorkplaceShiftComponent>()
                .Bind();

            this.BindFragment<WorkplaceShiftFragment>();
        }
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableWorkplace)).PatchAll();
    }

}
global using PowerShaftSwitch.Components;
global using PowerShaftSwitch.Services;

namespace PowerShaftSwitch;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        MultiBind<IPrefabModifier>().To<PrefabModifier>().AsSingleton();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            var b = new TemplateModule.Builder();
            b.AddDecorator<TransputSwitchComponentSpec, TransputSwitchComponent>();
            return b.Build();
        }).AsSingleton();

        Bind<TransputSwitchService>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(PowerShaftSwitch)).PatchAll();
    }

}

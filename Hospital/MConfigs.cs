global using Hospital.Components;
global using Hospital.Services;

namespace Hospital;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<HospitalMaterialService>().AsSingleton();
        
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<HospitalComponentSpec, HospitalComponent>();
            return b.Build();
        }).AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(Hospital)).PatchAll();
    }

}
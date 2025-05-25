global using Hospital.PrefabAdder;
global using Hospital.Components;
global using Hospital.Services;

namespace Hospital;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<HospitalAssetProvider>().AsSingleton();
        MultiBind<IAssetProvider>().ToExisting<HospitalAssetProvider>();
        MultiBind<ISpecModifier>().To<HospitalSpecAdder>().AsSingleton();
        MultiBind<IPrefabGroupServiceFrontRunner>().To<HospitalPrefabLoader>().AsSingleton();

        Bind<HospitalMaterialService>().AsSingleton();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<HospitalComponentSpec, HospitalComponent>();
            return b.Build();
        }).AsSingleton();
    }
}

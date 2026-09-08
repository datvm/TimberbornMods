#define REGISTER_TEST_NO

namespace ModdableBindito;

[Context("MainMenu")]
[Context("Game")]
[Context("MapEditor")]
public class ModdableBlueprintConfigurator : Configurator
{

    public override void Configure()
    {
        this.Remove<ISpecService>();
        Bind<ISpecService>().To<ModdableSpecService>().AsSingleton();

#if REGISTER_TEST
        this.MultiBindAndBindSingleton<ISpecServiceFrontRunner, TestSpecFrontRunner>();
#endif
    }

}

[Context("Game")]
[Context("MapEditor")]
public class ModdablePrefabGroupSystemConfigurator : Configurator
{
    public override void Configure()
    {
        this.Remove<PrefabGroupService>();
        Bind<PrefabGroupService>().To<PrefabGroupServiceWrapper>().AsSingleton();
    }
}
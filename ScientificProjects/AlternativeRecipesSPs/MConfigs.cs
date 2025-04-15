global using AlternativeRecipesSPs.Management;

namespace AlternativeRecipesSPs;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<ProjectRecipeDescriber>().AsSingleton();
        Bind<ProjectRecipeUnlocker>().AsSingleton();
        MultiBind<IDefaultRecipeLocker>().To<ProjectRecipeLocker>().AsSingleton();
    }
}

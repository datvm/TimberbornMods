namespace UnlockableRecipe;

[Context("Game")]
public class UnlockableRecipeGameConfig : Configurator
{

    class InstanceBindProvider(DefaultRecipeLockerController instance) : IProvider<ICustomRecipeLocker>
    {
        public ICustomRecipeLocker Get() => instance;
    }

    public override void Configure()
    {
        // Mod Recipe Managements
        Bind<RecipeLockerRegistry>().AsSingleton();
        Bind<RecipeLockerUi>().AsSingleton();
        Bind<DefaultRecipeLockerController>().AsSingleton();
        MultiBind<ICustomRecipeLocker>().ToProvider<InstanceBindProvider>().AsSingleton();

        // Inferior Treating
        Bind<InferiorTreatingUnlocker>().AsSingleton();
        MultiBind<IDefaultRecipeLocker>().To<InferiorTreatingLocker>().AsSingleton();
    }

}

[Context("MainMenu")]
public class UnlockableRecipeMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<RecipeModRegistry>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(UnlockableRecipe)).PatchAll();
    }

}
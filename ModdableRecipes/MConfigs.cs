global using ModdableRecipes.Services;
global using ModdableRecipes.Specs;
global using ModdableRecipes.Components;
global using ModdableRecipes.UI;

namespace ModdableRecipes;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<ModdableRecipeLockSpecService>()
            .BindSingleton<ModdableRecipePersistentUnlocker>()
            .BindSingleton<ModdableRecipeLockService>()
            .BindSingleton<ModdableRecipeLockInitializer>()
            .BindSingleton<LiveRecipeModifierService>()
            .BindSingleton<RecipeGoodsProcessorReference>()

            .MultiBindSingleton<IRecipeLockProvider, ModdableSpecLockProvider>()

            .BindSingleton<ModdableRecipeUIController>()

            .BindTemplateModule(h => h
                .AddDecorator<Manufactory, ModdableManufactoryDropdownProvider>()
            )
        ;
    }
}


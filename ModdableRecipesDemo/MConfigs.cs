global using ModdableRecipesDemo.UI;
global using ModdableRecipesDemo.Services;

namespace ModdableRecipesDemo;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<CustomRecipeDemoService>()

            .BindFragment<TestModdableRecipeFragment>()
        ;
    }
}

namespace ModdableRecipesDemo.Services;

public class CustomRecipeDemoService(LiveRecipeModifierService liveModifier) : ILoadableSingleton
{

    public void Load()
    {
        liveModifier.Modify("CustomPlank", s => s with
        {
            Ingredients = [
                new() { Id = "Water", Amount= 2 },
            ],
        });
    }

}

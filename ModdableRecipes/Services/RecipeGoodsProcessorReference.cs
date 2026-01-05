namespace ModdableRecipes.Services;

class RecipeGoodsProcessorReference : IUnloadableSingleton
{
    static RecipeGoodsProcessorReference? instance;
    public static RecipeGoodsProcessorReference Instance => instance ?? throw new InvalidOperationException("RecipeGoodsProcessorReference is not initialized.");

    public readonly RecipeGoodsProcessor Processor;

    public RecipeGoodsProcessorReference(RecipeGoodsProcessor processor)
    {
        instance = this;
        Processor = processor;
    }

    public void Unload()
    {
        instance = null;
    }
}

namespace PackagerBuilder.Services.GoodProviders;

public interface IGoodBuilderProvider
{
    bool ShouldProvide(PackagerBuildOptions options);
    IEnumerable<BuiltGood> ProvideGoods(GoodSpec g, PackagerBuildOptions options);
    AdditionalFactionData ProvideAdditionalData();
}

public record BuiltGood(
    GoodSpec? OriginalSpec,
    GoodSpec BuiltSpec,
    BuiltRecipesPair Recipes
)
{
    public string Id => BuiltSpec.Id;
    public string OriginalId => OriginalSpec?.Id ?? throw new InvalidOperationException("No OriginalSpec");
}

public record AdditionalFactionData(
    IReadOnlyList<string> GoodIds,
    BuiltRecipesPair Recipes,
    IReadOnlyList<BuildingPrefabRecipeAdd> RecipesForBuildings
)
{
    public static AdditionalFactionData Empty => new([], BuiltRecipesPair.Empty, []);

    public AdditionalFactionData(IEnumerable<AdditionalFactionData> data) : this(
        [.. data.SelectMany(d => d.GoodIds).Distinct()],
        new(
            [.. data.SelectMany(d => d.Recipes.PackagingRecipe).Distinct()],
            [.. data.SelectMany(d => d.Recipes.UnpackingRecipe).Distinct()]
        ),
        [.. data.SelectMany(d => d.RecipesForBuildings)]
    ) { }

}

public readonly record struct BuiltGoodRecipe(RecipeSpec Recipe, string Icon, string DisplayName);
public readonly record struct GoodNamePair(string Name, string PluralName);
public readonly record struct BuiltRecipesPair(IReadOnlyList<BuiltGoodRecipe> PackagingRecipe, IReadOnlyList<BuiltGoodRecipe> UnpackingRecipe)
{
    public static readonly BuiltRecipesPair Empty = new([], []);

    public IEnumerable<BuiltGoodRecipe> All => PackagingRecipe.Concat(UnpackingRecipe);

    public IReadOnlyList<BuiltGoodRecipe> GetList(bool isPackager) => isPackager ? PackagingRecipe : UnpackingRecipe;
    public IEnumerable<string> GetListIds(bool isPackager) => GetList(isPackager).Select(r => r.Recipe.Id);
}
public readonly record struct BuildingPrefabRecipeAdd(string Id, IReadOnlyList<string> Prefabs, IReadOnlyList<string> Recipes);